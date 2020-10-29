using NUnit.Framework;
using QWest.Apis;
using QWest.DataAcess;
using System;
using System.Threading.Tasks;

namespace QWest.Api.Tests {
    [TestFixture]
    public class ImageControllerSpec {
        public class ImageRepoMock : DAO.IImage {
            public Task<byte[]> Get(int id) {
                throw new NotImplementedException();
            }
        }
        [Test]
        public async Task NullReturnsValidImage() {
            ImageController imageController = new ImageController();
            imageController.ImageRepo = new ImageRepoMock();
            byte[] bytes = await (await imageController.Get(null)).Content.ReadAsByteArrayAsync();
            Assert.IsNotEmpty(bytes);
        }
    }
}
