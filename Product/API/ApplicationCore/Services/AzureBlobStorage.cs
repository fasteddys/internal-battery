using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;

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

            } catch(Exception e)
            {
                // todo: log
                return false;
            }
        }
    }
}
