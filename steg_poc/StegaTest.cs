using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace steg_poc
{
    [TestFixture]
    class StegaTest
    {
        [Test]
        [Ignore("Ignore sanity test")]
        public void SanityTest()
        {
            Assert.True(true, "Sanity test, this test must fail ;D");
        }

        [Test]
        //[Ignore("Ignore test")]
        public void EncodeProducesOutputFile()
        {
            using (FileStream fsSource = new FileStream(@"/home/ghost/Projects/steg_poc/steg_poc/program.jpg", 
                FileMode.Open, FileAccess.Read))
            {
                Stega stega = new Stega();
                byte[] fileBytes = new byte[fsSource.Length];

                fsSource.Read(fileBytes, 0, fileBytes.Length);
                fsSource.Close();
                stega.EncodeImage(fileBytes, new string[] {
                    @"/home/ghost/Projects/steg_poc/steg_poc/testpic.png",
                       @"/home/ghost/Projects/steg_poc/steg_poc/testpic1.png" });

                // When
                Assert.True(File.Exists(@"/home/ghost/Projects/steg_poc/steg_poc/output_0.png"));
                Assert.True(File.Exists(@"/home/ghost/Projects/steg_poc/steg_poc/output_1.png"));
            }
        }

        [Test]
        //[Ignore("Ignore test")]
        public void SHA1FileChecksum()
        {
            // Given
            string inputFileHash = String.Empty;
            string outputFileHash = String.Empty;

            using (FileStream fileStream = new FileStream(@"/home/ghost/Projects/steg_poc/steg_poc/program.jpg", FileMode.Open))
            using (BufferedStream buffStream = new BufferedStream(fileStream))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    // Given
                    byte[] hash = sha1.ComputeHash(buffStream);

                    // Then
                    StringBuilder formatted = new StringBuilder(2 * inputFileHash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }

                    inputFileHash = formatted.ToString();
                }

                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    // Given
                    Stega stega = new Stega();
                    byte[] imageFileBytes = stega.DecodeImage(new string[] {
                    @"/home/ghost/Projects/steg_poc/steg_poc/output_0.png",
                    @"/home/ghost/Projects/steg_poc/steg_poc/output_1.png" });
                    File.WriteAllBytes("testees.jpg", imageFileBytes);
                    byte[] hash = sha1.ComputeHash(imageFileBytes);

                    // Then
                    StringBuilder formatted = new StringBuilder(2 * outputFileHash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }

                    outputFileHash = formatted.ToString();
                }

                // When
                Assert.AreEqual(inputFileHash, outputFileHash);
            }
        }
    }
}
