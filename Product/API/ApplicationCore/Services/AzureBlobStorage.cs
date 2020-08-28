using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class AzureBlobStorage : ICloudStorage
    {
        private CloudStorageAccount _account;
        private CloudBlobClient _client;
        private string _containerName;
        private string _assetContainer;
        private string _appDataContainer;

        public AzureBlobStorage(IConfiguration config)
        {
            string storageConnectionString = config["StorageAccount:ConnectionString"];
            _containerName = config["StorageAccount:DefaultContainer"];
            _assetContainer = config["StorageAccount:AssetContainer"];
            _appDataContainer = config["StorageAccount:AppDataContainer"];
            if (!CloudStorageAccount.TryParse(storageConnectionString, out _account))
                throw new Exception("Unable to parse StorageAccount:ConnectionString");

            _client = _account.CreateCloudBlobClient();
        }

        public async Task<Stream> GetStreamAsync(string blobName)
        {         
            return await GetBlob(blobName).OpenReadAsync();
                
        }

        public async Task<Stream> OpenReadAsync(string blobName)
        {    
            return await GetBlob(blobName).OpenReadAsync();
        }

        public async Task DownloadToStreamAsync(string blobName, Stream stream)
        {
            await GetBlob(blobName).DownloadToStreamAsync(stream);
        }

        private CloudBlockBlob GetBlob(string blobName)
        {
            CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(_containerName);
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            return blob;
        }

        private string GetBlobName(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.UtcNow.Ticks, name, ext);
        }

        public async Task<string> UploadFileAsync(string path, string fileName, Stream fileStream)
        {
            CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(_containerName);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(path + GetBlobName(fileName));
            await blob.UploadFromStreamAsync(fileStream);

            return blob.Name;
        }


        public async Task<string> UploadBlobAsync(string blobName, byte[] blobArray)
        {
            CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(_assetContainer);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            Stream stream = new MemoryStream(blobArray);
            await blob.UploadFromStreamAsync(stream);

            return blob.Name;
        }



        private async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            return await UploadFileAsync(String.Empty, fileName, fileStream);
        }

        public async Task<bool> DeleteFileAsync(string blobName)
        {
            try
            {
                CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(_containerName);
                await cloudBlobContainer.CreateIfNotExistsAsync();

                CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
                await blob.DeleteAsync();
                return true;

            } catch(Exception)
            {
                // todo: log
                return false;
            }
        }

 


        // todo jab  

        public async Task<string> GetBlobSAS(string blobURI)
        {
            // TODO JAB confifify put in all app configs!!!!!!
            string StorageAccountVideoBaseUrl = "https://careercirclestaging.blob.core.windows.net/intro-videos/";
            string StorageAccountKey = "88XKuYvTrulmfSJxO5F7XB/Nal2/zDv2XbRi/QT0wdgT+XpzuxWH/AAkGyFqG1RnVyDEkNhR7bDZVG8jPaxo3w==";
            string StorageAccountName = "careercirclestaging";
            int VideoSASLifeTimeInMinutesForSubscriber = 10;
 

            // string containerName = "intro-videos/491c799b-a0dc-4b25-9cc3-dca17f914610";
            string blobName = Path.GetFileName(blobURI);
            string[] uriComponents = blobURI.Replace("//",string.Empty).Split('/');
            string containerName = uriComponents[1] + "/" + uriComponents[2];

            StorageSharedKeyCredential key = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);
 

            // Create a SAS token 
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName, 
                Resource = "b",
                Protocol = SasProtocol.Https,
            };

            sasBuilder.StartsOn = DateTimeOffset.UtcNow;
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(VideoSASLifeTimeInMinutesForSubscriber);

  
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read );
            
            // Use the key to get the SAS token.
            string sasVideoToken = sasBuilder.ToSasQueryParameters(key).ToString();
 
            return sasVideoToken;
        }

        public async Task<string> GetContainerSAS(string ContainerName)
        {
            // TODO JAB confifify put in all app configs!!!!!!
            string StorageAccountVideoBaseUrl = "https://careercirclestaging.blob.core.windows.net/intro-videos/";
            string StorageAccountKey = "88XKuYvTrulmfSJxO5F7XB/Nal2/zDv2XbRi/QT0wdgT+XpzuxWH/AAkGyFqG1RnVyDEkNhR7bDZVG8jPaxo3w==";
            string StorageAccountName = "careercirclestaging";
            int VideoSASLifeTimeInMinutesForContainer = 10;
            
            StorageSharedKeyCredential key = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);


            // Create a SAS token 
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = ContainerName, 
                Resource = "c",
                Protocol = SasProtocol.Https,
            };

            sasBuilder.StartsOn = DateTimeOffset.UtcNow;
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(VideoSASLifeTimeInMinutesForContainer);


            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // Use the key to get the SAS token.
            string sasVideoToken = sasBuilder.ToSasQueryParameters(key).ToString();

            return sasVideoToken;
        }





    }
}
