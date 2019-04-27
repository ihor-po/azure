using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*
                 * Getting configuration data for application
                 */
                string connectionString = ConfigurationManager.ConnectionStrings["AzureConnectionString"].ConnectionString;
                string localFolder = ConfigurationManager.AppSettings["sourceFolder"];
                string destConteiner = ConfigurationManager.AppSettings["destinationConteiner"];
                string downloadContainer = ConfigurationManager.AppSettings["containerForDownload"];

                Console.WriteLine(@"Connection to storage account");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                Console.WriteLine(@"Getting reference to conteiner");
                CloudBlobContainer container = blobClient.GetContainerReference(downloadContainer);

                // If container not exist - create container in blob storage
                container.CreateIfNotExists();

                // Retrieving files for uploads
                string[] fileEntries = Directory.GetFiles(localFolder);
                if (fileEntries.Count() > 0)
                {
                    foreach (string filePath in fileEntries)
                    {
                        //Create new file path for file
                        string key = DateTime.UtcNow.ToString("yyyy-MM-dd-HH_mm_ss") + "-" + Path.GetFileName(filePath);

                        UploadBlob(container, key, filePath, true);
                    }
                }
                
                DownloadBlob(container, localFolder);

            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Upload file to blob storage
        /// </summary>
        /// <param name="container">BLOB container</param>
        /// <param name="key">File path</param>
        /// <param name="fileName">File name in storage</param>
        /// <param name="deleteAfter">If need delete from folder after uploading</param>
        private async static void UploadBlob(CloudBlobContainer container, string key, string fileName, bool deleteAfter)
        {
            try
            {
                Console.WriteLine(@"Uploading file to container: key=" + key + " source file=" + fileName);

                //Get reference to blob object in BLOB container
                CloudBlockBlob blob = container.GetBlockBlobReference($"{DateTime.UtcNow.ToString("yyyy-MM-dd")}/{key}");

                //Saving file to blob
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    await blob.UploadFromStreamAsync(fs);
                }

                //Deleting file from local folder
                if (deleteAfter && fileName != ".gitkeep")
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        /// <summary>
        /// Download folders from BLOB storage 
        /// </summary>
        /// <param name="container">BLOB container</param>
        /// <param name="localFolder">Local main path where save downloaded files</param>
        private async static void DownloadBlob(CloudBlobContainer container, string localFolder)
        {
            BlobContinuationToken dirToken = null;
            do
            {
                var dirResult = await container.ListBlobsSegmentedAsync(dirToken);
                dirToken = dirResult.ContinuationToken;
                
                foreach (var dirItem in dirResult.Results)
                {
                    if (dirItem is CloudBlobDirectory)
                    {
                        var dir = dirItem as CloudBlobDirectory;
                        Console.WriteLine($"Start download directory:\n{dir.Prefix}");
                        
                        BlobContinuationToken blobToken = null;
                        var blobResult = await dir.ListBlobsSegmentedAsync(blobToken);

                        foreach (var blobItem in blobResult.Results)
                        {
                            if (blobItem is CloudBlockBlob)
                            {
                                var blob = blobItem as CloudBlockBlob;

                                string fileName = blob.Name;
                                fileName = fileName.Replace("/", "\\");

                                string folderName = fileName.Split('\\')[0];
                                Directory.CreateDirectory($"{localFolder}\\{folderName}");

                                Console.WriteLine($"Download file: {fileName} from blob");

                                blob.DownloadToFile($"{localFolder}/{fileName}", FileMode.Create);
                            }
                        }
                    }
                }
            } while (dirToken != null);
        }
    }
}
