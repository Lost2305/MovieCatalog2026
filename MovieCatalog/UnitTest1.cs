using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;
using TheMovieCatalogue.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace TheMovieCatalogue

{

	[TestFixture]
	public class MovieTests
	{
		private RestClient client;
		private static string movieId;


		private const string BaseUrl = "http://144.91.123.158:5000";
		private const string LoginEmail = "writtendirectino9@example.com";
		private const string LoginPassword = "feetlover";

		[OneTimeSetUp]
		public void Setup()
		{

			string jwtToken = GetJwtToken(LoginEmail, LoginPassword);

			RestClientOptions options = new RestClientOptions(BaseUrl)
			{
				Authenticator = new JwtAuthenticator(jwtToken)
			};
			this.client = new RestClient(options);
		}

		private string GetJwtToken(string email, string password)
		{
			RestClient client = new RestClient(BaseUrl);
			RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
			request.AddJsonBody(new { email, password });

			RestResponse response = client.Execute(request);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
				var token = content.GetProperty("accessToken").GetString();

				if (string.IsNullOrWhiteSpace(token))
				{
					throw new InvalidOperationException("Token not found in the response.");

				}
				return token;
			}
			else
			{
				throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
			}
		}


		[Order(1)]
		[Test]
		public void CreateNewMovie_WithRequiredFields_ShouldReturnSuccess()
		{

			MovieDto movie = new MovieDto
			{
				Title = "I Swear",
				Description = "John Davidson: diagnosed with Tourette's syndrome at a young age which alienated him from his peers.",

			};

			RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
			request.AddJsonBody(movie);

			RestResponse response = client.Execute(request);

			ApiResponseDto createResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(createResponse.Movie, Is.Not.Null);
			Assert.That(createResponse.Movie.Id, Is.Not.Null);
			Assert.That(createResponse.Movie.Id, Is.Not.Empty);
			Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));


			movieId = createResponse.Movie.Id;


		}

		[Order(2)]
		[Test]
		public void EditMovieTitle_ShouldReturnSuccess()
		{

			MovieDto movieEdit = new MovieDto
			{
				Title = "I Swear Edited",
				Description = "John Davidson now loves swearing."

			};

			RestRequest request = new RestRequest("/api/Movie/Edit/", Method.Put);
			request.AddQueryParameter("movieId", movieId);
			request.AddJsonBody(movieEdit);

			RestResponse response = client.Execute(request);

			ApiResponseDto editResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(editResponse.Msg, Is.EqualTo("Movie edited successfully!"));


		}

		[Order(3)]
		[Test]
		public void GetAllMovies_ShouldReturnNonEmptyArray()
		{

			RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
			RestResponse response = client.Execute(request);

			List<ApiResponseDto> createResponse = JsonSerializer.Deserialize<List<ApiResponseDto>>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(createResponse, Is.Not.Null);
			Assert.That(createResponse, Is.Not.Empty);
			Assert.That(createResponse.Count, Is.GreaterThanOrEqualTo(1));


		}

		[Order(4)]
		[Test]
		public void DeleteExistingMovie_ShouldSucceed()
		{
			RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
			request.AddQueryParameter("movieId", movieId);
			RestResponse response = client.Execute(request);


			ApiResponseDto deleteResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(deleteResponse.Msg, Is.EqualTo("Movie deleted successfully!"));

		}

		[Order(5)]
		[Test]
		public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
		{
			MovieDto movieCreate = new MovieDto
			{
				Title = "",
				Description = ""

			};
			RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
			request.AddJsonBody(movieCreate);

			RestResponse response = client.Execute(request);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

		}

		[Order(6)]
		[Test]
		public void EditNonExistentMovie_ShouldReturnBadRequest()
		{

			string nonExistentMovieId = "01189998819991197253";

			MovieDto movieEdit = new MovieDto
			{
				Title = "I Swear Part 2",
				Description = "John Davidson is now all mute. He can't even swear anymore"

			};

			RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
			request.AddQueryParameter("movieId", nonExistentMovieId);
			request.AddJsonBody(movieEdit);

			RestResponse response = client.Execute(request);

			ApiResponseDto editResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(editResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
		}


		[Order(7)]
		[Test]
		public void DeleteNonExistentMovie_ShouldReturnBadRequest()
		{
			string nonExistentMovieId = "01189998819991197253";

			RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
			request.AddQueryParameter("movieId", nonExistentMovieId);

			RestResponse response = client.Execute(request);

			ApiResponseDto deleteResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(deleteResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));

		}


		[OneTimeTearDown]
		public void TearDown()
		{
			this.client?.Dispose();
		}
	}
}







