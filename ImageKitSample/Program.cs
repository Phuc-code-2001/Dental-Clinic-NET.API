﻿using Imagekit;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ImageKitSample
{
    class Program
    {
        [ExcludeFromCodeCoverage]
        static async Task Main(string[] args)
        {

            string publicKey = "public_aUyELt2YVwpWP00c2S97uv2X2ps";
            string privateKey = "private_/ruWnqNpOopwwW6SAQxUxNjADLA=";
            string urlEndPoint = "https://ik.imagekit.io/sdrpji7cj/";

            ServerImagekit imagekit = new ServerImagekit(publicKey, privateKey, urlEndPoint, "path");

            Console.WriteLine("Imagekit initialized!");

            /// Upload File
            string imagePath = @"D:\FPT\FALL 2022\PRN231\Dental-Clinic-NET\ImageKitSample\test.jpg";
            //var uploadResp = await imagekit.FileName("test.jpg").UploadAsync(imagePath);
            //Console.WriteLine(JToken.FromObject(uploadResp).ToString());


            /// Upload base64 file
            var fileInfo = new FileInfo(imagePath);
            var uploadBase64Resp = imagekit.UseUniqueFileName()
                .FileName("test.jpg")
                .Upload(File.ReadAllBytes(fileInfo.FullName));
            Console.WriteLine(JToken.FromObject(uploadBase64Resp).ToString());


            /// Upload By URL Cai lon thang code nhu cc
            var imgURL = "https://ik.imagekit.io/demo/default-image.jpg?tr=h-100,w-200";
            var uploadByURLResp = imagekit
                .FileName(imgURL)
                .UseUniqueFileName(false)
                .Tags("tag1,tag2,tag3")
                .Upload(imagePath);
            Console.WriteLine(JToken.FromObject(uploadByURLResp));


            /// listing Files
            var fileList = imagekit
                .Skip(0)
                .Limit(3)
                .Sort("DESC_SIZE")
                .SearchQuery("tags IN [\"tag1\"]")
                .ListFiles();
            foreach (var val in fileList)
            {
                Console.WriteLine(JToken.FromObject(val));
            }


            /// Generating URLs
            string path = "/default-image.jpg";
            Transformation trans = new Transformation().Width(400).Height(300);
            string imageURL = imagekit.Url(trans).Path(path).TransformationPosition("query").Generate();
            Console.WriteLine("Url for first image transformed with height: 300, width: 400 - {0}", imageURL);


            /// Generating Signed URL
            var imgURL1 = "https://ik.imagekit.io/demo/default-image.jpg";
            string[] queryParams = { "b=123", "a=test" };
            try
            {
                var signedUrl = imagekit.Url(new Transformation().Width(400).Height(300))
                .Src(imgURL1)
                .QueryParameters(queryParams)
                .ExpireSeconds(600)
                .Signed()
                .Generate();
                Console.WriteLine("Signed Url for first image transformed with height: 300, width: 400: - {0}", signedUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            /// Get File Details
            var fileId = fileList[0].FileId;
            var fileDetailResp = imagekit.GetFileDetails(fileId);
            Console.WriteLine(JToken.FromObject(fileDetailResp));


            /// Get File Metadata
            var MetadataResp1 = imagekit.GetFileMetadata(fileId);
            var MetadataResp2 = imagekit.GetFileMetadata(fileList[1].FileId);
            Console.WriteLine(JToken.FromObject(MetadataResp1));


            /// pHash Distance
            Console.WriteLine(MetadataResp1.PHash, MetadataResp2.PHash);
            var pHashDistance = imagekit.PHashDistance(MetadataResp1.PHash, MetadataResp2.PHash);
            Console.WriteLine("pHash Distance: {0}", pHashDistance);


            /// Update file details
            string[] tags = { "tag1", "tag2" };
            var FileUpdateResp = imagekit.Tags(tags).CustomCoordinates("10,10,100,100").UpdateFileDetails(fileId);
            Console.WriteLine(JToken.FromObject(FileUpdateResp));


            /// Purge cache & purge cache status
            var purgeCacheResponse = imagekit.PurgeCache(imgURL1);
            Console.WriteLine("Cache purge request id: {0}", purgeCacheResponse.RequestId);

            var purgeCacheStatus = imagekit.GetPurgeCacheStatus(purgeCacheResponse.RequestId);
            Console.WriteLine("Cache purge status: {0}", JToken.FromObject(purgeCacheStatus));


            /// Delete File
            var deleteResp = imagekit.DeleteFileAsync(fileId);
            Console.WriteLine(JToken.FromObject(deleteResp).ToString());


            /// Get Authentication Token
            var authenticationParameters = imagekit.GetAuthenticationParameters("your_token");
            Console.WriteLine("Authentication Parameters: {0}", JToken.FromObject(authenticationParameters).ToString());


        }
    }
}
