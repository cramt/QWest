using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using QWest.Api.Controllers;
using QWest.Apis;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.Api.Tests {
    [TestFixture]
    public class ImageControllerSpec {
        public class ImageRepoMock : DAO.IImage {
            public Dictionary<int, byte[]> ImageDictionary { get; set; }
            public Task<byte[]> Get(int id) {
                return Task.FromResult(ImageDictionary[id]);
            }
        }
        [Test]
        public async Task NullReturnsValidImage() {
            ImageController imageController = new ImageController();
            imageController.ImageRepo = new ImageRepoMock();
            byte[] bytes = ((FileContentResult)await imageController.Get(null)).FileContents;
            Assert.IsNotEmpty(bytes);
        }
        [Test]
        public async Task ReturnsCorrectImage() {
            byte[] expected = new byte[] { 1, 2, 3 };
            ImageController imageController = new ImageController();
            imageController.ImageRepo = new ImageRepoMock {
                ImageDictionary = new Dictionary<int, byte[]> {
                    {5, expected }
                }
            };
            byte[] bytes = ((FileContentResult)await imageController.Get(5)).FileContents;
            Assert.AreEqual(expected, bytes);
        }
    }
}
