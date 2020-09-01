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
        private IConfiguration _configuration;


        public AzureBlobStorage(IConfiguration config)
        {
            string storageConnectionString = config["StorageAccount:ConnectionString"];
            _containerName = config["StorageAccount:DefaultContainer"];
            _assetContainer = config["StorageAccount:AssetContainer"];
            _appDataContainer = config["StorageAccount:AppDataContainer"];
            if (!CloudStorageAccount.TryParse(storageConnectionString, out _account))
                throw new Exception("Unable to parse StorageAccount:ConnectionString");

            _client = _account.CreateCloudBlobClient();
            _configuration = config;
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

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RenameFileAsync(string oldFilePath, string newFilePath, bool overWrite)
        {
            string oldBlobName = Path.GetFileName(oldFilePath);
            string newBlobName = Path.GetFileName(newFilePath);
            string[] oldFileUriComponent = oldFilePath.Replace("//", string.Empty).Split('/');
            string[] newFileUriComponent = newFilePath.Replace("//", string.Empty).Split('/');
            string oldFileBlob =  oldFileUriComponent[2] + "/" + oldBlobName;
            string newFileBlob =  newFileUriComponent[2] + "/" + newBlobName;

            CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(oldFileUriComponent[1]);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(oldFileBlob);
            CloudBlockBlob blobCopy = cloudBlobContainer.GetBlockBlobReference(newFileBlob);

            if (await blob.ExistsAsync())
            {
                if (await blobCopy.ExistsAsync() && overWrite)
                {
                    await blobCopy.DeleteIfExistsAsync();
                }
                await blobCopy.StartCopyAsync(blob);
                await blob.DeleteIfExistsAsync();
            }
            return true;
        }


        public async Task<string> GetBlobSAS(string blobURI)
        {
            string StorageAccountKey = _configuration["StorageAccount:StorageAccountKey"];
            string StorageAccountName = _configuration["StorageAccount:StorageAccountName"];
            int VideoSASLifeTimeInMinutesForSubscriber = int.Parse(_configuration["StorageAccount:VideoSASLifeTimeInMinutesForSubscriber"]);

            string blobName = Path.GetFileName(blobURI);
            string[] uriComponents = blobURI.Replace("//", string.Empty).Split('/');
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
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // Use the key to get the SAS token.
            string sasVideoToken = sasBuilder.ToSasQueryParameters(key).ToString();

            return sasVideoToken;
        }

        public async Task<string> GetVideoContainerSAS()
        {
            string VideoContainer = _configuration["StorageAccount:VideoContainer"];
            string StorageAccountKey = _configuration["StorageAccount:StorageAccountKey"];
            string StorageAccountName = _configuration["StorageAccount:StorageAccountName"];
            int VideoSASLifeTimeInMinutesForContainer = int.Parse(_configuration["StorageAccount:VideoSASLifeTimeInMinutesForContainer"]);

            StorageSharedKeyCredential key = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);

            // Create a SAS token 
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = VideoContainer,
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
