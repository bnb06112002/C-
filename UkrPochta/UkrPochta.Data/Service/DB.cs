using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UkrPochta.Data.Entity;
using Windows.Storage;

namespace UkrPochta.Data.Service
{
   public class DB
    {
        private SqliteContext _context;
        private string Urldownload = "http://services.ukrposhta.com/postindex_new/upload/houses.zip";
        private string fileDownload = "houses.zip";
        private string fileData = "houses.csv";
        public bool IsOpen;
        public bool IsEmpty;
        public DB(string dbPath)
        {
            _context = new SqliteContext(dbPath);
            IsOpen = _context.IsOpen;
           
            IsEmpty = IsOpen && (GetCountRegion() == 0);

        }

        public int GetCountRegion()
        {
            return _context.Region.Count();
        }
        public Region GetRegion(int regionId)
        {
            if (!IsOpen)
                return null;
            return _context.Region.SingleOrDefault(d => d.Id == regionId);
        }

        public IList<Region> GetRegion(string search, int start, int end)
        {
            if (!IsOpen)
                return null;

            return
                _context.Region
                    .OrderBy(d => d.Name)
                    .Skip(start)
                    .Take(end - start)
                    .ToList();
        }

        public IList<Region> GetRegion()
        {
            if (!IsOpen)
                return null;

            return
                _context.Region
                    .OrderBy(d => d.Name)                    
                    .ToList();
        }

       

        public IList<District> GetDistrict(int idRegion)
        {
            if (!IsOpen)
                return null;

            return
                _context.District.Where(d=>d.RegionId==idRegion)
                    .OrderBy(d => d.Name)
                    .ToList();
        }
        public IList<City> GetCity(int idDistrict)
        {
            if (!IsOpen)
                return null;

            return
                _context.City.Where(d => d.DistrictId == idDistrict)
                    .OrderBy(d => d.Name)
                    .ToList();
        }
        public IList<Street> GetStreet(int idCity)
        {
            if (!IsOpen)
                return null;

            return
                _context.Street.Where(d => d.CityId == idCity)
                    .OrderBy(d => d.Name)
                    .ToList();
        }
        public async Task<bool> UpdateDate(StorageFolder tmpFolder, IProgress<string> loggedText)
        {
           var fl= await tmpFolder.GetFilesAsync();
            foreach(StorageFile f in fl)
            {
                loggedText.Report("del: " + f.DisplayName.ToString());                   
               
               await f.DeleteAsync();

            }
            loggedText.Report("download: ");
            var downloadFile = await BackgroundDownloadAsync(tmpFolder,loggedText);
            if (downloadFile == null)
                return false;
           
           
            ZipFile.ExtractToDirectory(downloadFile.Path, tmpFolder.Path);
            var x = await tmpFolder.TryGetItemAsync(fileData);
            if (x != null)
            {
              var xr = await Task.Run(()=> CsvDataImport(x.Path, loggedText));                
                return xr;
            }
            else return false;
        }

        private bool AcceptData(IProgress<string> loggedText)
        {
           
                _context.Db.BeginTransaction();
                _context.Db.Execute("delete from tbl_region");
                _context.Db.Execute("delete from tbl_district");
                _context.Db.Execute("delete from tbl_city");
                _context.Db.Execute("delete from tbl_street");
                _context.Db.Execute("delete from tbl_house");
                string _updRegion = "insert into tbl_region(name) select distinct region from tbl_data";
                string _updDistrict = "insert into tbl_district(regionid,name) " +
                                      "select distinct r.id,d.district from tbl_data d " +
                                      "left join tbl_region r on r.name = d.region";
                string _updCity = "insert into tbl_city(districtid,name) " +
                                  "select distinct _d.id,d.city from tbl_data d " +
                                  "left join tbl_region r on r.name = d.region " +
                                  "left join tbl_district _d on _d.name = d.district and _d.regionid = r.id";
                string _updStreet = "insert into tbl_street(cityid,name) " +
                                    "select distinct c.id,d.street from tbl_data d " +
                                    "left join tbl_region r on r.name = d.region " +
                                    "left join tbl_district _d on _d.name = d.district and _d.regionid = r.id " +
                                    "left join tbl_city c on c.name = d.city and c.districtid = _d.id";
                loggedText.Report("accept data Region");
                _context.Db.Execute(_updRegion);
                loggedText.Report("accept data District");
                _context.Db.Execute(_updDistrict);
                loggedText.Report("accept data City");
                _context.Db.Execute(_updCity);
                loggedText.Report("accept data Street");
                _context.Db.Execute(_updStreet);
                _context.Db.Commit();
                
                return true;
            
           
        }

        private async Task<bool> CsvDataImport(string path, IProgress<string> loggedText)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            loggedText.Report("read file: " + path);
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding("windows-1251"));

            var numLines = 0;
            _context.Db.BeginTransaction();

            _context.Db.Execute("delete from tbl_data");
            foreach (string line in lines)
            {
                numLines++;
                if (numLines == 1)
                    continue;
                var l = line.Replace("\"", "'");
                string[] cols = l.Split(new[] { ';' }, StringSplitOptions.None);
                string insStr = "insert into tbl_Data (region,district,city,postindex,street,house) values(\"" + cols[0].ToString() + "\",\"" + cols[1].ToString() + "\",\"" + cols[2].ToString() + "\",\"" + cols[3].ToString() + "\",\"" + cols[4].ToString() + "\",\"" + cols[5].ToString() + "\")";
                //  insStr=string.Format(insStr, cols);
                var cmd = _context.Db.CreateCommand(insStr);
                cmd.ExecuteNonQuery();
                loggedText.Report("added " + numLines + " of " + lines.Count());
                
            }
            _context.Db.Commit();
            var x = await Task.Run(() => AcceptData(loggedText));
            return x;
        }

        private async Task<StorageFile> BackgroundDownloadAsync(StorageFolder tmpFolder, IProgress<string> loggedText)
        {
            loggedText.Report("download: " + Urldownload);
            Uri _url;
            if (!Uri.TryCreate(Urldownload, UriKind.Absolute, out _url))
                return null;
            StorageFile localFile = await tmpFolder.CreateFileAsync(fileDownload, CreationCollisionOption.GenerateUniqueName);
            try
            {               
                HttpClient client = new HttpClient();
                byte[] buffer = await client.GetByteArrayAsync(_url);
                using (Stream stream = await localFile.OpenStreamForWriteAsync())
                { stream.Write(buffer, 0, buffer.Length);
                    loggedText.Report("download: " + stream.Length.ToString());
                }
                return localFile;
            }
            catch
            {
                await localFile.DeleteAsync();
                return null;
            }
        }
    }
}
