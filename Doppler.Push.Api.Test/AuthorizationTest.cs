using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.Push.Api.Test
{
    public class AuthorizationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        const string TOKEN_EMPTY = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.e30.Nbd00AAiP2vJjxr78oPZoPRsDml5dx2bdD1Y6SXomfZN8pzJdKel2zrplvXCGBBYNBOo90rdYSlBCCo15rxsVydiFcAP84qZv-2mh4pFED9tVyDbxV5hvYDSg2bHPFyYFAi26fJusu_oYY3ne8OWxx-W1MEzNxh2hPfEKTkd0zVBm4dZv_irizRpa_qBwjn3hbCLUtOhBFbTTFItM9hESo6RwHvtQaB0667Sj8N97-bleCY5Ppf6bUUMz2A35PDb8-roF5Scf97lTZfug_DymgpPRSNK2VcRjfAynKfbBSih4QqVeaxR5AhYtXVFbQgByrynYNLok1SFD-M48WpzSA";
        const string TOKEN_BROKEN = "eyJhbGciOiJSzI1NiIsInR5cCI6IkpXVCJ9.e0.Nbd00AAiP2vJjxr8oPZoPRsDml5dx2bdD1Y6SXomfZN8pzJdKel2zrplvXCGBBYNBOo90rdYSlBCCo15rxsVydiFcAP84qZv-2mh4pFED9tVyDbxV5hvYDSg2bHPFyYFAi26fJusu_oYY3ne8OWxx-W1MEzNxh2hPfEKTkd0zVBm4dZv_irizRpa_qBwjn3hbCLUtOhBFbTTFItM9hESo6RwHvtQaB0667Sj8N97-bleCY5Ppf6bUUMz2A35PDb8-roF5Scf97lTZfug_DymgpPRSNK2VcRjfAynKfbBSih4QqVeaxR5AhYtXVbQgByrynYNLok1SFD-M48WpzSA";
        const string TOKEN_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwMDAwMDAwMDB9.mll33c0kstVIN9Moo4HSw0CwRjn0IuDc2h1wkRrv2ahQtIG1KV5KIxYw-H3oRfd-PiCWHhIVIYDP3mWDZbsOHTlnpRGpHp4f26LAu1Xp1hDJfOfxKYEGEE62Xt_0qp7jSGQjrx-vQey4l2mNcWkOWiE0plOws7cX-wLUvA3NLPoOvEegjM0Wx6JFcvYLdMGcTGT5tPd8Pq8pe9VYstCbhOClzI0bp81iON3f7VQP5d0n64eb_lvEPFu5OfURD4yZK2htyQK7agcNNkP1c5mLEfUi39C7Qtx96aAhOjir6Wfhzv_UEs2GQKXGTHl6_-HH-ecgOdIvvbqXGLeDmTkXUQ";
        const string TOKEN_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjEwMDAwMDAwMDB9.ZRjcfFTB973pD_gwB562BLCcszQOzubvr9TP6pWgA4wVIPeCzsX4waH7J9LPydY3pkp0UxaOffv-vJO0xZoWE9eUHdQbk8sy1CBgFM_dgyxY7DHKt0vuSjkPQ-VryPYwrTXO5lvaaDtMXIz6NdGC62oFQbvNOWD60790g2xzloge1bLpBYT1YRJK5dblA_mG9IJ1Id4R1HIZEmOIkOIhGU8-GQx2bP82xpudcEjOUZS7buRHpSy_Oy6fjy1KfUND_IbePuNF_t4n8Qo-MahshaphJrZlIKpEbw9gqlviH5s4lyU7AHhEs0JoTb2RGNTLq9h6m4Y-eMEFmPXnWN6dAA";
        const string TOKEN_SUPERUSER_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        const string TOKEN_SUPERUSER_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjEwMDAwMDAwMDB9.FYOpOxrXSHDif3lbQLPEStMllzEktWPKQ2T4vKUq5qgVjiH_ki0W0Ansvt0PMlaLHqq7OOL9XGFebtgUcyU6aXPO9cZuq6Od196TWDLMdnxZ-Ct0NxWxulyMbjTglUiI3V6g3htcM5EaurGvfu66kbNDuHO-WIQRYFfJtbm7EuOP7vYBZ26hf5Vk5KvGtCWha4zRM55i1-CKMhXvhPN_lypn6JLENzJGYHkBC9Cx2DwzaT683NWtXiVzeMJq3ohC6jvRpkezv89QRes2xUW4fRgvgRGQvaeQ4huNW_TwQKTTikH2Jg7iHbuRqqwYuPZiWuRkjqfd8_80EdlSAnO94Q";
        const string TOKEN_SUPERUSER_FALSE_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjpmYWxzZSwiZXhwIjoyMDAwMDAwMDAwfQ.qMY3h8VhNxuOBciqrmXpTrRk8ElwDlT_3CYFzqJdXNjnJhKihFVMwjkWVw1EEckCWbKsRoBr-NgRV0SZ0JKWbMr2oGhZJWtqmKA05d8-i_MuuYbxtt--NUoQxg6AsMX989PGf6fSBzo_4szb7J0G6nUvvRxXfMnHMpaIAQUiBLNOoeKwnzsZFfI1ehmYGNmtc-2XyXOEHAnfZeBZw8uMWOp4A5hFBpVsaVCUiRirokjeCMWViVWT9NnVWbA60e_kfLjghEcXWaZfNnX9qtj4OC8QUB33ByUmwuYlTxNnu-qiEaJmbaaTeDD2JrKHf6MR59MlCHbb6BDWt20DBy73WQ";


        private readonly WebApplicationFactory<Startup> _factory;

        public AuthorizationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/Message", TOKEN_EMPTY, HttpStatusCode.Unauthorized)]
        [InlineData("/Message", TOKEN_BROKEN, HttpStatusCode.Unauthorized)]
        [InlineData("/Message", TOKEN_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
        [InlineData("/Message", TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/Message", TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
        [InlineData("/Message", TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/Message", TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        public async Task POST_authenticated_endpoints_should_require_valid_token(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
