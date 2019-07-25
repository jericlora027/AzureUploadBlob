using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AzureUploadBlob
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = ConfigurationManager.ConnectionStrings["sampleresourcegroup27"].ConnectionString;
            string localFolder = ConfigurationManager.AppSettings["sourceFolder"];
            string destContainer= ConfigurationManager.AppSettings["destContainer"];
            string strAction;

            Console.WriteLine(@"Connecting to storage account");
            CloudStorageAccount sa = CloudStorageAccount.Parse(connString);
            CloudBlobClient bc = sa.CreateCloudBlobClient();
            CloudBlobContainer container = bc.GetContainerReference(destContainer);

            container.CreateIfNotExists();

            var blobs = container.ListBlobs();

            string[] fileEntries = Directory.GetFiles(localFolder);
            foreach (string filePath in fileEntries)
            {
                
                string key = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss") + "-" + Path.GetFileName(filePath);
                UploadBlob(container, key, filePath, MimeMapping.GetMimeMapping(filePath), true);

            }

            Console.WriteLine(@"Uploading complete., \n");
            Console.WriteLine(@"Press D to download files in Blob Storage: ");

            strAction = Console.ReadLine();

            if (strAction.ToUpper() == "D")
            {
                DownloadBlobs(blobs);
            }

            Console.WriteLine(@"Press any key to exit.");
            Console.ReadKey();


        }

        //Method for Download files in Blob Storage
        private static void DownloadBlobs(IEnumerable<IListBlobItem> iBlobs)
        {
            var destFolder = ConfigurationManager.AppSettings["destFolder"];

            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            foreach (var blob in iBlobs)
            {
                if (blob is CloudBlockBlob blockBlob)
                {
                    
                    blockBlob.DownloadToFile(destFolder + blockBlob.Name,FileMode.Create);
                    Console.WriteLine(blockBlob.Name);
                }
                else if (blob is CloudBlobDirectory blobDirectory)
                {
                    Directory.CreateDirectory(destFolder + blobDirectory.Prefix);
                    Console.WriteLine("Create Directory " + destFolder + blobDirectory.Prefix);

                    DownloadBlobs(blobDirectory.ListBlobs());
                }
            }


        }

        static void UploadBlob(CloudBlobContainer container, string key, string fileName, string pathFileType ,bool deleteAfter)
        {
            Console.WriteLine(@"Uploading file in container: key="+ key + " source file= "+ fileName);
            CloudBlockBlob b = container.GetBlockBlobReference(key);

            using (var fs = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                b.Properties.ContentType = pathFileType;
                b.UploadFromStream(fs);
                
               
            }

           // if (deleteAfter)
              //  File.Delete(fileName);
        }
    }
}
