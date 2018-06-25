using System;
using System.Linq;
using ColourCoded.Security.API.Controllers;
using ColourCoded.Security.API.Data;
using ColourCoded.Security.API.Models.RequestModels.Login;
using ColourCoded.Security.API.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ColourCoded.Security.API.Tests
{
  [TestClass]
  public class AuthenticateControllerTests
  {
    private class Resources
    {
      public AuthenticateController Controller;
      public SecurityContext Context;
      public IConfiguration Configuration;
      public MemoryCache MemoryCache;
      public Mock<ISecurityHelper>  SecurityHelper;
      public Mock<ICommunicationsHelper> CommunicationsHelper;

      public Resources()
      {
        Configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

        MemoryCache = new MemoryCache(new MemoryCacheOptions());
        SecurityHelper = new Mock<ISecurityHelper>();
        CommunicationsHelper = new Mock<ICommunicationsHelper>();
        Context = TestHelper.CreateDbContext(Configuration);
        Controller = new AuthenticateController(Context, MemoryCache, Configuration, SecurityHelper.Object, CommunicationsHelper.Object);
      }

      public void AddMockSaltHashAlgorithm(string password, string salt, string hashPassword)
      {
        SecurityHelper.Setup(x => x.SaltedHashAlgorithm(password, salt)).Returns(hashPassword);
      }

      public void AddMockValidatePassword(string password, string hashPassword, bool result)
      {
        SecurityHelper.Setup(x => x.ValidatePassword(password, hashPassword)).Returns(result);
      }

      public void AddMockCreateRandomPassword(int length, string newPassword)
      {
        SecurityHelper.Setup(x => x.CreateRandomPassword(length)).Returns(newPassword);
      }

      public void AddMockSendEmail(string emailAddress, string subject, string bodyText)
      {
        CommunicationsHelper.Setup(x => x.SendEmail(emailAddress, subject, bodyText)).Returns(true);
      }
    }

    [TestMethod]
    public void Register_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new RegisterUserRequestModel {
                                                          Username = "testuser",
                                                          Password = "password",
                                                          ConfirmPassword = "password",
                                                          FirstName = "Test",
                                                          LastName = "User",
                                                          EmailAddress = "test@gmail.com"
                                                        };

        // When
        var result = resources.Controller.Register(requestModel);

        // Then
        var savedUser = resources.Context.Users.First(u => u.Username == requestModel.Username);
        Assert.AreEqual(requestModel.FirstName, savedUser.FirstName);
        Assert.AreEqual(requestModel.LastName, savedUser.LastName);
        Assert.AreEqual(requestModel.EmailAddress, savedUser.EmailAddress);
      }
    }

    [TestMethod]
    public void Register_ValidateUsername_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new ValidateUserRequestModel
        {
          Username = "testuser"
        };

        // When
        var result = resources.Controller.FindUsername(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsValid);
        var savedUser = resources.Context.Users.FirstOrDefault(u => u.Username == requestModel.Username);
        Assert.IsNull(savedUser);
      }
    }

    [TestMethod]
    public void Register_ValidateUsername_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveUsers(resources.Context);
        var user = TestHelper.CreateUser(resources.Context, username: "testuser");

        var requestModel = new ValidateUserRequestModel
        {
          Username = user.Username
        };

        // When
        var result = resources.Controller.FindUsername(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Username already exists", result.Messages[0].Message);

        var savedUser = resources.Context.Users.First(u => u.Username == requestModel.Username);
        Assert.IsNotNull(savedUser);
      }
    }

    [TestMethod]
    public void Register_ValidateEmail_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new ValidateEmailRequestModel
        {
          EmailAddress = "test@gmail.com"
        };

        // When
        var result = resources.Controller.FindUserEmail(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsValid);
        var savedUser = resources.Context.Users.FirstOrDefault(u => u.EmailAddress == requestModel.EmailAddress);
        Assert.IsNull(savedUser);
      }
    }

    [TestMethod]
    public void Register_ValidateEmail_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveUsers(resources.Context);
        var user = TestHelper.CreateUser(resources.Context, username: "testuser", emailAddress: "test@gmail.com");

        var requestModel = new ValidateEmailRequestModel
        {
          EmailAddress = user.EmailAddress
        };

        // When
        var result = resources.Controller.FindUserEmail(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Already linked to another user.", result.Messages[0].Message);

        var savedUser = resources.Context.Users.First(u => u.EmailAddress == requestModel.EmailAddress);
        Assert.IsNotNull(savedUser);
      }
    }

    [TestMethod]
    public void ForgottenPassword_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var user = TestHelper.CreateUser(resources.Context);
        var requestModel = new CredentialsRequestModel
        {
          EmailAddress = "test@gmail.com"
        };

        resources.AddMockCreateRandomPassword(Convert.ToInt32(resources.Configuration["Security.TempPasswordLength"]), "testpassword");

        var emailBody = @"Your temporary password is: testpassword" + Environment.NewLine +
                 "Please click on the following link to change your password: " + resources.Configuration["ColourCoded.UI.Sitename"] + "/security/authenticate/changepassword?username=" + user.Username;

        resources.AddMockSendEmail(user.EmailAddress, "Password reset", emailBody);

        // When
        var result = resources.Controller.ForgotPassword(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
      }
    }

    [TestMethod]
    public void ForgottenPassword_NoEmail()
    { 
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new CredentialsRequestModel
        {
          EmailAddress = "test@gmail.com"
        };

        // When
        var result = resources.Controller.ForgotPassword(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
      }
    }

    [TestMethod]
    public void Login_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        const string password = "password";
        var user = TestHelper.CreateUser(resources.Context, password: password);
        var requestModel = new LoginRequestModel
        {
          Username = user.Username,
          Password = "password"
        };
        resources.AddMockValidatePassword(password, password, true);

        // When
        var result = resources.Controller.Login(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsAuthenticated);
        Assert.AreEqual(user.Username, result.Username);
      }
    }

    [TestMethod]
    public void Login_UsernameNotExist()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new LoginRequestModel { Username = "testuser", Password = "password" };

        // When
        var result = resources.Controller.Login(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsAuthenticated);
        Assert.IsNull(result.Username);
      }
    }

    [TestMethod]
    public void Login_IncorrectPassword()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var user = TestHelper.CreateUser(resources.Context, password: "password");
        var requestModel = new LoginRequestModel { Username = user.Username, Password = "wrongpasswod" };

        resources.AddMockValidatePassword("wrongpasswod", "password", false);

        // When
        var result = resources.Controller.Login(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsAuthenticated);
        Assert.AreEqual(user.Username, result.Username);
      }
    }

    [TestMethod]
    public void ChangePassword_Successful()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        const string password = "password";
        var user = TestHelper.CreateUser(resources.Context, password: password);
        var requestModel = new ChangePasswordRequestModel
        {
          Username = user.Username,
          NewPassword = "newpassword"
        };

        // When
        var result = resources.Controller.ChangePassword(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
      }
    }
  }
}
