﻿namespace Paseto.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    using NUnit.Framework;

    using Algorithms;
    using Builder;
    using Cryptography;
    using Extensions;
    using Protocol;
    using Utils;
    using static Utils.EncodingHelper;

    [TestFixture]
    public class PasetoTests
    {
        private const string HelloPaseto = "Hello Paseto!";
        private const string IssuedBy = "Paragon Initiative Enterprises";
        private const string PublicKeyV1 = "<RSAKeyValue><Modulus>2Q3n8GRPEbcxAtT+uwsBnY08hhJF+Fby0MM1v5JbwlnQer7HmjKsaS97tbfnl87BwF15eKkxqHI12ntCSezxozhaUrgXCGVAXnUmZoioXTdtJgapFzBob88tLKhpWuoHdweRu9yGcWW3pD771zdFrRwa3h5alC1MAqAMHNid2D56TTsRj4CAfLSZpSsfmswfmHhDGqX7ZN6g/TND6kXjq4fPceFsb6yaKxy0JmtMomVqVTW3ggbVJhqJFOabwZ83/DjwqWEAJvfldz5g9LjvuislO5mJ9QEHBu7lnogKuX5g9PRTqP3c6Kus0/ldZ8CZvwWpxnxnwMRH10/UZ8TepQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string TokenV1 = "v1.public.eyJleGFtcGxlIjoiSGVsbG8gUGFzZXRvISIsImV4cCI6IjE1MjEzMDc1MzMifTzjEcgP2a3p_IrMPuU9bH8OvOmV5Olr8DFK3rFu_7SngF_pZ0cU1X9w590YQeZTy37B1bPouoXZDQ9JDYBfalxG0cNn2aP4iKHgYuyrOqHaUTmbNeooKOvDPwwl6CFO3spTTANLK04qgPJnixeb9mvjby2oM7Qpmn28HAwwr_lSoOMPhiUSCKN4u-SA6G6OddQTuXY-PCV1VtgQA83f0J6Yy3x7MGH9vvqonQSuOG6EGLHJ09p5wXllHQyGZcRm_654aKpwh8CXe3w8ol3OfozGCMFF_TLo_EeX0iKSkE8AQxkrQ-Fe-3lP_t7xPkeNhJPnhAa0-DGLSFQIILsL31M";
        private const string PublicKeyV2 = "rJRRV5JmY3BRUmyWu2CRa1EnUSSNbOgrAMTIsgbX3Z4=";
        private const string TokenV2 = "v2.public.eyJleGFtcGxlIjoiSGVsbG8gUGFzZXRvISIsImV4cCI6IjIwMTgtMDQtMDdUMDU6MDQ6MDcuOTE5NjM3NVoifTuR3EYYCG12DjhIqPKiVmTkKx2ewCDrYNZHcoewiF-lpFeaFqKW3LkEgnW28UZxrBWA5wrLFCR5FP1qUlMeqQA";
        private const string LocalKeyV2 = "37ZJdkLlZ43aF8UO7GWqi7GrdO0zDZSpSFLNTAdmKdk=";
        private const string LocalTokenV2 = "v2.local.ENG98mfmCWo7p8qEha5nuyv4lP5y8248m9GasN_K5Yw2-CJksfXlbnEsTQHSMi49pqRzpvDTfo705J1ol98tc2e2Up62_4stDlPZQLAAwDeAQK0tS14h8JSYYunq3kvkeVTq6aNyCdw";
        private const string LocalTokenWithFooterV2 = "v2.local.ENG98mfmCWo7p8qEha5nuyv4lP5y8248m9GasN_K5Yw2-CJksfXlbnEsTQHSMi49pqRzpvDTfo705J1ol98tc2e2Up62_4stDlPZQLAAwDeAQK0tS14h8PyCfJzDW_mg6Bky_oW2HZw.eyJraWQiOiJnYW5kYWxmMCJ9";
        private const string ExpectedPublicPayload = "{\"example\":\"Hello Paseto!\",\"exp\":\"2018-04-07T05:04:07.9196375Z\"}";
        private const string ExpectedLocalPayload = "{\"example\":\"Hello Paseto!\",\"exp\":\"2018-04-07T04:57:18.5865183Z\"}";
        private const string ExpectedFooter = "{\"kid\":\"gandalf0\"}";

        #region Version 1
#if NETCOREAPP2_1 || NET47

        [Test]
        public void Version1SignatureTest()
        {
            // Arrange
            var paseto = new Version1();

            string key = null;
#if NETCOREAPP2_1
            using (var rsa = RSA.Create())
                key = rsa.ToCompatibleXmlString(true);
#elif NET47
            using (var rsa = new RSACng())
                key = rsa.ToXmlString(true);
#endif

            var sk = GetBytes(key);

            // Act
            var token = paseto.Sign(sk, HelloPaseto);

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version1SignatureVerificationTest()
        {
            // Arrange
            var paseto = new Version1();

            string key = null;
            string pubKey = null;
#if NETCOREAPP2_1
            using (var rsa = RSA.Create())
            {
                //rsa.KeySize = 2048; // Default

                key = rsa.ToCompatibleXmlString(true);
                pubKey = rsa.ToCompatibleXmlString(false);
            }
#elif NET47
            using (var rsa = new RSACng())
            {
                //rsa.KeySize = 2048; // Default

                key = rsa.ToXmlString(true);
                pubKey = rsa.ToXmlString(false);
            }
#endif
            var sk = GetBytes(key);
            var pk = GetBytes(pubKey);

            // Act
            var token = paseto.Sign(sk, HelloPaseto);
            var verified = paseto.Verify(token, pk).Valid;

            // Assert
            Assert.IsTrue(verified);
        }

        [Test]
        public void Version1BuilderTokenGenerationTest()
        {
            // Arrange
            string key = null;
#if NETCOREAPP2_1
            using (var rsa = RSA.Create())
                key = rsa.ToCompatibleXmlString(true);
#elif NET47
            using (var rsa = new RSACng())
                key = rsa.ToXmlString(true);
#endif

            // Act
            var token = new PasetoBuilder<Version1>()
                              .WithKey(GetBytes(key))
                              .AddClaim("example", HelloPaseto)
                              .Expiration(DateTime.UtcNow.AddHours(24))
                              .AsPublic()
                              .Build();

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version1BuilderTokenDecodingTest()
        {
            // Arrange & Act
            var payload = new PasetoBuilder<Version1>()
                              .WithKey(GetBytes(PublicKeyV1))
                              .AsPublic()
                              .AndVerifySignature()
                              .Decode(TokenV1);

            // Assert
            Assert.IsNotNull(payload);
        }
#endif

        #endregion

        #region Version 2

        [Test]
        public void Version2SignatureTest()
        {
            // Arrange
            var paseto = new Version2();
            var seed = new byte[32];
            Ed25519.KeyPairFromSeed(out var pk, out var sk, seed);

            // Act
            var signature = paseto.Sign(sk, HelloPaseto);

            // Assert
            Assert.IsNotNull(signature);
        }

        [Test]
        public void Version2SignatureNullSecretFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Sign(null, HelloPaseto));
        }

        [Test]
        public void Version2SignatureEmptySecretFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => paseto.Sign(new byte[0], HelloPaseto));
        }

        [Test]
        public void Version2SignatureNullPayloadFails()
        {
            // Arrange
            var paseto = new Version2();
            Ed25519.KeyPairFromSeed(out var pk, out var sk, new byte[32]);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Sign(sk, null));
        }

        [Test]
        public void Version2SignatureEmptyPayloadFails()
        {
            // Arrange
            var paseto = new Version2();
            Ed25519.KeyPairFromSeed(out var pk, out var sk, new byte[32]);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Sign(sk, string.Empty));
        }

        [Test]
        public void Version2SignatureVerificationTest()
        {
            // Arrange
            var paseto = new Version2();
            var seed = new byte[32];
            RandomNumberGenerator.Create().GetBytes(seed);
            Ed25519.KeyPairFromSeed(out var pk, out var sk, seed);

            //var pub = Convert.ToBase64String(pk);

            // Act
            var token = paseto.Sign(sk, HelloPaseto);
            var verified = paseto.Verify(token, pk).Valid;

            // Assert
            Assert.IsTrue(verified);
        }

        [Test]
        public void Version2SignatureVerificationNullTokenFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Verify(null, null));
        }

        [Test]
        public void Version2SignatureVerificationEmptyTokenFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Verify(string.Empty, null));
        }

        [Test]
        public void Version2SignatureVerificationNullPublicKeyFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => paseto.Verify(TokenV2, null));
        }

        [Test]
        public void Version2SignatureVerificationEmptyPublicKeyFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => paseto.Verify(TokenV2, new byte[0]));
        }

        [Test]
        public void Version2SignatureVerificationInvalidPublicKeyFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => paseto.Verify(TokenV2, new byte[16]));
        }

        [Test]
        public void Version2SignatureVerificationInvalidTokenHeaderVersionFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => paseto.Verify("v1.public.", new byte[32]));
        }

        [Test]
        public void Version2SignatureVerificationInvalidTokenHeaderFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => paseto.Verify("v2.remote.", new byte[32]));
        }

        [Test]
        public void Version2SignatureVerificationInvalidTokenBodyFails()
        {
            // Arrange
            var paseto = new Version2();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => paseto.Verify("v2.public.eyJleGFtcGxlIjoiSGVsbG8gUGFzZX", new byte[32]));
        }

        [Test]
        public void Version2BuilderPublicTokenGenerationTest()
        {
            // Arrange
            var seed = new byte[32]; // signingKey
            RandomNumberGenerator.Create().GetBytes(seed);
            var sk = Ed25519.ExpandedPrivateKeyFromSeed(seed);

            //var secret = Convert.ToBase64String(sk); //BitConverter.ToString(sk).Replace("-", string.Empty); // Hex Encoded

            // Act
            var token = new PasetoBuilder<Version2>()
                              .WithKey(sk)
                              .AddClaim("example", HelloPaseto)
                              .Expiration(DateTime.UtcNow.AddHours(24))
                              .AsPublic()
                              .Build();

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version2BuilderLocalTokenGenerationTest()
        {
            // Arrange
            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);

            //key = Convert.FromBase64String(LocalKeyV2);

            // Act
            var token = new PasetoBuilder<Version2>()
                              .WithKey(key)
                              .AddClaim("example", HelloPaseto)
                              .Expiration(DateTime.UtcNow.AddHours(24))
                              //.Expiration(DateTime.Parse("2018-04-07T04:57:18.5865183Z").ToUniversalTime())
                              .AsLocal()
                              .Build();

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version2BuilderLocalTokenWithFooterGenerationTest()
        {
            // Arrange
            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);

            //key = Convert.FromBase64String(LocalKeyV2);

            // Act
            var token = new PasetoBuilder<Version2>()
                              .WithKey(key)
                              .AddClaim("example", HelloPaseto)
                              .Expiration(DateTime.UtcNow.AddHours(24))
                              //.Expiration(DateTime.Parse("2018-04-07T04:57:18.5865183Z").ToUniversalTime())
                              .AddFooter(new PasetoPayload { { "kid", "gandalf0" } })
                              .AsLocal()
                              .Build();

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version2BuilderTokenGenerationNullSecretFails() => Assert.Throws<InvalidOperationException>(() => new PasetoBuilder<Version2>().WithKey(null).Build());

        [Test]
        public void Version2BuilderTokenGenerationEmptySecretFails() => Assert.Throws<InvalidOperationException>(() => new PasetoBuilder<Version2>().WithKey(new byte[0]).Build());

        [Test]
        public void Version2BuilderTokenGenerationEmptyPayloadFails()
        {
            // Arrange
            var seed = new byte[32]; // signingKey
            RandomNumberGenerator.Create().GetBytes(seed);
            var sk = Ed25519.ExpandedPrivateKeyFromSeed(seed);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new PasetoBuilder<Version2>().WithKey(sk).Build());
        }

        [Test]
        public void Version2BuilderPublicTokenDecodingTest()
        {
            // Arrange & Act
            var payload = new PasetoBuilder<Version2>()
                              .WithKey(Convert.FromBase64String(PublicKeyV2))
                              .AsPublic()
                              .Decode(TokenV2);

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo(ExpectedPublicPayload));
        }

        [Test]
        public void Version2BuilderLocalTokenDecodingTest()
        {
            // Arrange & Act
            var payload = new PasetoBuilder<Version2>()
                              .WithKey(Convert.FromBase64String(LocalKeyV2))
                              .AsLocal()
                              .Decode(LocalTokenV2);

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo(ExpectedLocalPayload));
        }

        [Test]
        public void Version2BuilderLocalTokenWithFooterDecodingTest()
        {
            // Arrange & Act
            var payload = new PasetoBuilder<Version2>()
                              .WithKey(Convert.FromBase64String(LocalKeyV2))
                              .AsLocal()
                              .Decode(LocalTokenWithFooterV2);

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo(ExpectedLocalPayload));
        }

        [Test]
        public void Version2BuilderLocalTokenWithFooterDecodingToObjectTest()
        {
            // Arrange & Act
            var data = new PasetoBuilder<Version2>()
                           .WithKey(Convert.FromBase64String(LocalKeyV2))
                           .AsLocal()
                           .DecodeToObject(LocalTokenWithFooterV2);

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void Version2BuilderLocalTokenWithFooterDecodingFooterOnlyTest()
        {
            // Arrange & Act
            var footer = new PasetoBuilder<Version2>().DecodeFooter(LocalTokenWithFooterV2);

            // Assert
            Assert.IsNotNull(footer);
            Assert.That(footer, Is.EqualTo(ExpectedFooter));
        }

        [Test]
        public void Version2BuilderTokenDecodingNullKeyFails() => Assert.Throws<InvalidOperationException>(() => new PasetoBuilder<Version2>().WithKey(null).Decode(null));

        [Test]
        public void Version2BuilderTokenDecodingEmptyKeyFails() => Assert.Throws<InvalidOperationException>(() => new PasetoBuilder<Version2>().WithKey(new byte[0]).Decode(null));

        [Test]
        public void Version2BuilderTokenDecodingNullTokenFails() => Assert.Throws<ArgumentNullException>(() => new PasetoBuilder<Version2>().WithKey(new byte[32]).AsPublic().Decode(null));

        [Test]
        public void Version2BuilderTokenDecodingEmptyTokenFails() => Assert.Throws<ArgumentNullException>(() => new PasetoBuilder<Version2>().WithKey(new byte[32]).AsPublic().Decode(string.Empty));

        [Test]
        public void Version2BuilderTokenDecodingInvalidTokenFails() => Assert.Throws<SignatureVerificationException>(() => new PasetoBuilder<Version2>().WithKey(Convert.FromBase64String(PublicKeyV2)).AsPublic().Decode("v2.public.eyJleGFtcGxlIjoiSGVsbG8gUGFzZXRvISIsImV2cCI6IjE1MjEyNDU0NTAifQ2jznA4Tl8r2PM8xu0FIJhyWkm4SiwvCxavTSFt7bo7JtnsFdWgXBOgbYybi5-NAkmpm94uwJCRjCApOXBSIgs"));

        [Test]
        public void Version2EncoderPublicPurposeTest()
        {
            // Arrange
            var seed = new byte[32]; // signingKey
            RandomNumberGenerator.Create().GetBytes(seed);
            var sk = Ed25519.ExpandedPrivateKeyFromSeed(seed);

            //var secret = Convert.ToBase64String(sk); //BitConverter.ToString(sk).Replace("-", string.Empty); // Hex Encoded

            // Act
            var encoder = new PasetoEncoder(cfg => cfg.Use<Version2>(sk)); // defaul is public purpose
            var token = encoder.Encode(new PasetoPayload
            {
                { "example", HelloPaseto },
                { "exp", DateTime.UtcNow.AddHours(24) }
            });

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version2DecoderPublicPurposeTest()
        {
            // Arrange & Act
            var decoder = new PasetoDecoder(cfg => cfg.Use<Version2>(Convert.FromBase64String(PublicKeyV2))); // default is public purpose
            var payload = decoder.Decode(TokenV2);

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo(ExpectedPublicPayload));
        }

        [Test]
        public void Version2EncoderLocalPurposeTest()
        {
            // Arrange
            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);

            //var secret = Convert.ToBase64String(key); //BitConverter.ToString(key).Replace("-", string.Empty); // Hex Encoded

            // Act
            var encoder = new PasetoEncoder(cfg => cfg.Use<Version2>(key, Purpose.Local));
            var token = encoder.Encode(new PasetoPayload
            {
                { "example", HelloPaseto },
                { "exp", DateTime.UtcNow.AddHours(24) }
            });

            // Assert
            Assert.IsNotNull(token);
        }

        [Test]
        public void Version2DecoderLocalPurposeTest()
        {
            // Arrange & Act
            var decoder = new PasetoDecoder(cfg => cfg.Use<Version2>(Convert.FromBase64String(LocalKeyV2), Purpose.Local));
            var payload = decoder.Decode(LocalTokenV2);

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo(ExpectedLocalPayload));
        }

        [Test]
        public void Version2DecoderToObjectLocalPurposeTest()
        {
            // Arrange & Act
            var decoder = new PasetoDecoder(cfg => cfg.Use<Version2>(Convert.FromBase64String(LocalKeyV2), Purpose.Local));
            var data = decoder.DecodeToObject(LocalTokenWithFooterV2);

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void Version2DecoderPublicPurposeFromPasetoExampleTest()
        {
            // Arrange & Act
            var decoder = new PasetoDecoder(cfg => cfg.Use<Version2>(CryptoBytes.FromHexString("11324397f535562178d53ff538e49d5a162242970556b4edd950c87c7d86648a"), Purpose.Public));
            var payload = decoder.Decode("v2.public.eyJleHAiOiIyMDM5LTAxLTAxVDAwOjAwOjAwKzAwOjAwIiwiZGF0YSI6InRoaXMgaXMgYSBzaWduZWQgbWVzc2FnZSJ91gC7-jCWsN3mv4uJaZxZp0btLJgcyVwL-svJD7f4IHyGteKe3HTLjHYTGHI1MtCqJ-ESDLNoE7otkIzamFskCA");

            // Assert
            Assert.IsNotNull(payload);
            Assert.That(payload, Is.EqualTo("{\"exp\":\"2039-01-01T00:00:00+00:00\",\"data\":\"this is a signed message\"}"));
        }

        #endregion

        #region Payload Validation

        [Test]
        public void PayloadNotBeforeNextDayValidationFails()
        {
            var nbf = new Validators.NotBeforeValidator(new PasetoPayload
            {
                { RegisteredClaims.NotBefore.GetRegisteredClaimName(), DateTime.UtcNow.AddHours(24) }
            });
            Assert.Throws<TokenValidationException>(() => nbf.Validate(DateTime.UtcNow), "Token is not yet valid.");
        }

        [Test]
        public void PayloadNotBeforeValidationTest()
        {
            var nbf = new Validators.NotBeforeValidator(new PasetoPayload
            {
                { RegisteredClaims.NotBefore.GetRegisteredClaimName(), DateTime.UtcNow.AddHours(-24) }
            });
            Assert.DoesNotThrow(() => nbf.Validate(DateTime.UtcNow));
        }

        [Test]
        public void PayloadExpirationTimeYesterdayValidationFails()
        {
            var exp = new Validators.ExpirationTimeValidator(new PasetoPayload
            {
                { RegisteredClaims.ExpirationTime.GetRegisteredClaimName(), DateTime.UtcNow.AddHours(-24) }
            });
            Assert.Throws<TokenValidationException>(() => exp.Validate(DateTime.UtcNow), "Token has expired.");
        }

        [Test]
        public void PayloadExpirationTimeValidationTest()
        {
            var exp = new Validators.ExpirationTimeValidator(new PasetoPayload
            {
                { RegisteredClaims.ExpirationTime.GetRegisteredClaimName(), DateTime.UtcNow.AddHours(24) }
            });
            Assert.DoesNotThrow(() => exp.Validate(DateTime.UtcNow));
        }

        [Test]
        public void PayloadEqualValidationNonEqualFails()
        {
            var val = new Validators.EqualValidator(new PasetoPayload
            {
                { RegisteredClaims.Issuer.GetRegisteredClaimName(), IssuedBy }
            }, RegisteredClaims.Issuer.GetRegisteredClaimName());
            Assert.Throws<TokenValidationException>(() => val.Validate(IssuedBy + "."));
        }

        [Test]
        public void PayloadEqualValidationTest()
        {
            var val = new Validators.EqualValidator(new PasetoPayload
            {
                { RegisteredClaims.Issuer.GetRegisteredClaimName(), IssuedBy }
            }, RegisteredClaims.Issuer.GetRegisteredClaimName());
            Assert.DoesNotThrow(() => val.Validate(IssuedBy));
        }

        [Test]
        public void PayloadCustomValidationNonEqualFails()
        {
            var val = new Validators.EqualValidator(new PasetoPayload
            {
                { "example", HelloPaseto }
            }, "example");
            Assert.Throws<TokenValidationException>(() => val.Validate(HelloPaseto + "!"));
        }

        [Test]
        public void PayloadCustomValidationTest()
        {
            var val = new Validators.EqualValidator(new PasetoPayload
            {
                { "example", HelloPaseto }
            }, "example");
            Assert.DoesNotThrow(() => val.Validate(HelloPaseto));
        }

        #endregion

        #region Test Vectors

        /*
        [Test]
        public void Version2LocalTestVector()
        {
            // Arrange
            foreach (var test in PasetoLocalTestVector.PasetoLocalTestVectors)
            {
                // Act
                var paseto = new Version2();
                var token = paseto.Encrypt(test.PrivateKey, test.Nonce, test.Message, test.Footer);

                // Assert
                Assert.That(token, Is.EqualTo(test.Token));
            }
        }
        */

        [Test]
        public void Version2PublicTestVector()
        {
            // Arrange
            foreach (var test in PasetoPublicTestVector.PasetoPublicTestVectors)
            {
                // Act
                var paseto = new Version2();
                var token = paseto.Sign(test.PrivateKey, test.Message, test.Footer);

                // Assert
                Assert.That(token, Is.EqualTo(test.Token));
            }
        }

        /*
        [Test]
        public void Version2EncryptTestVector()
        {
            // Arrange
            var symmetricKey = CryptoBytes.FromHexString("707172737475767778797a7b7c7d7e7f808182838485868788898a8b8c8d8e8f");

            var nullKey = CryptoBytes.FromHexString("0000000000000000000000000000000000000000000000000000000000000000");
            var fullKey = CryptoBytes.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var nonce = CryptoBytes.FromHexString("000000000000000000000000000000000000000000000000");
            var nonce2 = CryptoBytes.FromHexString("45742c976d684ff84ebdc0de59809a97cda2f64c84fda19b");

            var paseto = new Version2();

            // Test Vector 2E-1-1: Empty message, empty footer, empty nonce
            var token = paseto.Encrypt(nullKey, nonce, string.Empty, string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNUtKpdy5KXjKfpSKrOlqQvQ"));

            // Test Vector 2E-1-2
            token = paseto.Encrypt(fullKey, nonce, string.Empty, string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNSOvpveyCsjPYfe9mtiJDVg"));

            // Test Vector 2E-1-3
            token = paseto.Encrypt(symmetricKey, nonce, string.Empty, string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNkIWACdHuLiJiW16f2GuGYA"));

            // Test Vector 2E-2-1: Empty message, non-empty footer, empty nonce
            token = paseto.Encrypt(nullKey, nonce, string.Empty, "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNfzz6yGkE4ZxojJAJwKLfvg.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-2-2
            token = paseto.Encrypt(fullKey, nonce, string.Empty, "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNJbTJxAGtEg4ZMXY9g2LSoQ.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-2-3
            token = paseto.Encrypt(symmetricKey, nonce, string.Empty, "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.driRNhM20GQPvlWfJCepzh6HdijAq-yNreCcZAS0iGVlzdHjTf2ilg.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-3-1: Non-empty message, empty footer, empty nonce
            token = paseto.Encrypt(nullKey, nonce, "Love is stronger than hate or fear", string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.BEsKs5AolRYDb_O-bO-lwHWUextpShFSvu6cB-KuR4wR9uDMjd45cPiOF0zxb7rrtOB5tRcS7dWsFwY4ONEuL5sWeunqHC9jxU0"));

            // Test Vector 2E-3-2
            token = paseto.Encrypt(fullKey, nonce, "Love is stronger than hate or fear", string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.BEsKs5AolRYDb_O-bO-lwHWUextpShFSjvSia2-chHyMi4LtHA8yFr1V7iZmKBWqzg5geEyNAAaD6xSEfxoET1xXqahe1jqmmPw"));

            // Test Vector 2E-3-3
            token = paseto.Encrypt(symmetricKey, nonce, "Love is stronger than hate or fear", string.Empty);
            Assert.That(token, Is.EqualTo("v2.local.BEsKs5AolRYDb_O-bO-lwHWUextpShFSXlvv8MsrNZs3vTSnGQG4qRM9ezDl880jFwknSA6JARj2qKhDHnlSHx1GSCizfcF019U"));

            // Test Vector 2E-4-1: Non-empty message, non-empty footer, non-empty nonce
            token = paseto.Encrypt(nullKey, nonce2, "Love is stronger than hate or fear", "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.FGVEQLywggpvH0AzKtLXz0QRmGYuC6yvbcqXgWxM3vJGrJ9kWqquP61Xl7bz4ZEqN5XwH7xyzV0QqPIo0k52q5sWxUQ4LMBFFso.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-4-2
            token = paseto.Encrypt(fullKey, nonce2, "Love is stronger than hate or fear", "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.FGVEQLywggpvH0AzKtLXz0QRmGYuC6yvZMW3MgUMFplQXsxcNlg2RX8LzFxAqj4qa2FwgrUdH4vYAXtCFrlGiLnk-cHHOWSUSaw.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-4-3
            token = paseto.Encrypt(symmetricKey, nonce2, "Love is stronger than hate or fear", "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.local.FGVEQLywggpvH0AzKtLXz0QRmGYuC6yvl05z9GIX0cnol6UK94cfV77AXnShlUcNgpDR12FrQiurS8jxBRmvoIKmeMWC5wY9Y6w.Q3VvbiBBbHBpbnVz"));

            // Test Vector 2E-5
            token = paseto.Encrypt(symmetricKey, nonce2, "{\"data\":\"this is a signed message\",\"expires\":\"2019-01-01T00:00:00+00:00\"}", "Paragon Initiative Enterprises");
            Assert.That(token, Is.EqualTo("v2.local.lClhzVOuseCWYep44qbA8rmXry66lUupyENijX37_I_z34EiOlfyuwqIIhOjF-e9m2J-Qs17Gs-BpjpLlh3zf-J37n7YGHqMBV6G5xD2aeIKpck6rhfwHpGF38L7ryYuzuUeqmPg8XozSfU4PuPp9o8.UGFyYWdvbiBJbml0aWF0aXZlIEVudGVycHJpc2Vz"));

            // Test Vector 2E-6
            token = paseto.Encrypt(symmetricKey, nonce2, "{\"data\":\"this is a signed message\",\"exp\":\"2019-01-01T00:00:00+00:00\"}", "{\"kid\":\"zVhMiPBP9fRf2snEcT7gFTioeA9COcNy9DfgL1W60haN\"}");
            Assert.That(token, Is.EqualTo("v2.local.5K4SCXNhItIhyNuVIZcwrdtaDKiyF81-eWHScuE0idiVqCo72bbjo07W05mqQkhLZdVbxEa5I_u5sgVk1QLkcWEcOSlLHwNpCkvmGGlbCdNExn6Qclw3qTKIIl5-zSLIrxZqOLwcFLYbVK1SrQ.eyJraWQiOiJ6VmhNaVBCUDlmUmYyc25FY1Q3Z0ZUaW9lQTlDT2NOeTlEZmdMMVc2MGhhTiJ9"));
        }
        */

        [Test]
        public void Version2SignTestVector()
        {
            // Arrange
            var privateKey = CryptoBytes.FromHexString("b4cbfb43df4ce210727d953e4a713307fa19bb7d9f85041438d9e11b942a37741eb9dbbbbc047c03fd70604e0071f0987e16b28b757225c11f00415d0e20b1a2");
            var paseto = new Version2();

            // Test Vector S-1: Empty string, 32-character NUL byte key.
            //var token = paseto.Sign(privateKey, string.Empty, string.Empty);
            //Assert.That(token, Is.EqualTo("v2.public.xnHHprS7sEyjP5vWpOvHjAP2f0HER7SWfPuehZ8QIctJRPTrlZLtRCk9_iNdugsrqJoGaO4k9cDBq3TOXu24AA"));

            // Test Vector S-2: Empty string, 32-character NUL byte key, non-empty footer.
            //var token = paseto.Sign(privateKey, string.Empty, "Cuon Alpinus");
            //Assert.That(token, Is.EqualTo("v2.public.Qf-w0RdU2SDGW_awMwbfC0Alf_nd3ibUdY3HigzU7tn_4MPMYIKAJk_J_yKYltxrGlxEdrWIqyfjW81njtRyDw.Q3VvbiBBbHBpbnVz"));

            // Test Vector S-3: Non-empty string, 32-character 0xFF byte key.
            var token = paseto.Sign(privateKey, "Frank Denis rocks", string.Empty);
            Assert.That(token, Is.EqualTo("v2.public.RnJhbmsgRGVuaXMgcm9ja3NBeHgns4TLYAoyD1OPHww0qfxHdTdzkKcyaE4_fBF2WuY1JNRW_yI8qRhZmNTaO19zRhki6YWRaKKlCZNCNrQM"));

            // Test Vector S-4: Non-empty string, 32-character 0xFF byte key. (One character difference)
            token = paseto.Sign(privateKey, "Frank Denis rockz", string.Empty);
            Assert.That(token, Is.EqualTo("v2.public.RnJhbmsgRGVuaXMgcm9ja3qIOKf8zCok6-B5cmV3NmGJCD6y3J8fmbFY9KHau6-e9qUICrGlWX8zLo-EqzBFIT36WovQvbQZq4j6DcVfKCML"));

            // Test Vector S-5: Non-empty string, 32-character 0xFF byte key, non-empty footer.
            token = paseto.Sign(privateKey, "Frank Denis rocks", "Cuon Alpinus");
            Assert.That(token, Is.EqualTo("v2.public.RnJhbmsgRGVuaXMgcm9ja3O7MPuu90WKNyvBUUhAGFmi4PiPOr2bN2ytUSU-QWlj8eNefki2MubssfN1b8figynnY0WusRPwIQ-o0HSZOS0F.Q3VvbiBBbHBpbnVz"));

            // Test Vector S-6
            token = paseto.Sign(privateKey, "{\"data\":\"this is a signed message\",\"expires\":\"2019-01-01T00:00:00+00:00\"}", string.Empty);
            Assert.That(token, Is.EqualTo("v2.public.eyJkYXRhIjoidGhpcyBpcyBhIHNpZ25lZCBtZXNzYWdlIiwiZXhwaXJlcyI6IjIwMTktMDEtMDFUMDA6MDA6MDArMDA6MDAifSUGY_L1YtOvo1JeNVAWQkOBILGSjtkX_9-g2pVPad7_SAyejb6Q2TDOvfCOpWYH5DaFeLOwwpTnaTXeg8YbUwI"));

            token = paseto.Sign(privateKey, "{\"data\":\"this is a signed message\",\"expires\":\"2019-01-01T00:00:00+00:00\"}", "Paragon Initiative Enterprises");
            Assert.That(token, Is.EqualTo("v2.public.eyJkYXRhIjoidGhpcyBpcyBhIHNpZ25lZCBtZXNzYWdlIiwiZXhwaXJlcyI6IjIwMTktMDEtMDFUMDA6MDA6MDArMDA6MDAifcMYjoUaEYXAtzTDwlcOlxdcZWIZp8qZga3jFS8JwdEjEvurZhs6AmTU3bRW5pB9fOQwm43rzmibZXcAkQ4AzQs.UGFyYWdvbiBJbml0aWF0aXZlIEVudGVycHJpc2Vz"));

            // Test Vector 2E-6
            token = paseto.Sign(privateKey, "{\"data\":\"this is a signed message\",\"exp\":\"2019-01-01T00:00:00+00:00\"}", "{\"kid\":\"zVhMiPBP9fRf2snEcT7gFTioeA9COcNy9DfgL1W60haN\"}");
            Assert.That(token, Is.EqualTo("v2.public.eyJkYXRhIjoidGhpcyBpcyBhIHNpZ25lZCBtZXNzYWdlIiwiZXhwIjoiMjAxOS0wMS0wMVQwMDowMDowMCswMDowMCJ9flsZsx_gYCR0N_Ec2QxJFFpvQAs7h9HtKwbVK2n1MJ3Rz-hwe8KUqjnd8FAnIJZ601tp7lGkguU63oGbomhoBw.eyJraWQiOiJ6VmhNaVBCUDlmUmYyc25FY1Q3Z0ZUaW9lQTlDT2NOeTlEZmdMMVc2MGhhTiJ9"));
        }

        #endregion
    }
}
