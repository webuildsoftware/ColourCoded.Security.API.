using Microsoft.VisualStudio.TestTools.UnitTesting;
using ColourCoded.Security.API.Controllers;
using ColourCoded.Security.API.Data;
using Microsoft.Extensions.Configuration;
using ColourCoded.Security.API.Models.RequestModels.Permission;
using System.Linq;
using Moq;

namespace ColourCoded.Security.API.Tests
{
  [TestClass]
  public class PermissionsControllerTests
  {
    private class Resources
    {
      public PermissionsController GuiController;
      public SecurityContext DbContext;
      public IConfiguration IConfiguration;

      public Resources()
      {
        IConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

        DbContext = TestHelper.CreateDbContext(IConfiguration);
        GuiController = new PermissionsController(DbContext);
      }
    }

    [TestMethod]
    public void GetAll_ArtifactsList()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveArtifacts(resources.DbContext);
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        TestHelper.CreateArtifact(resources.DbContext);
        TestHelper.CreateArtifact(resources.DbContext);

        // When
        var result = resources.GuiController.GetAll();

        // Then
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Any(r => r.ArtifactName == artifact.ArtifactName));
        Assert.IsTrue(result.Any(r => r.ArtifactId == artifact.ArtifactId));
      }
    }

    [TestMethod]
    public void AddArtifact_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveArtifacts(resources.DbContext);
        var requestModel = new AddArtifactRequestModel { ArtifactName = "Test Artifact", CreateUser = "testuser" };

        // When
        var result = resources.GuiController.AddArtifact(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedRole = resources.DbContext.Artifacts.First(r => r.ArtifactName == requestModel.ArtifactName);
      }
    }

    [TestMethod]
    public void AddArtifact_ValidationResult_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        const string artifactName = "Test Artifact";
        var existingArtifact = TestHelper.CreateArtifact(resources.DbContext, artifactName);
        var requestModel = new AddArtifactRequestModel { ArtifactName = artifactName };

        // When
        var result = resources.GuiController.AddArtifact(requestModel);

        // Then
        Assert.IsFalse(result.IsValid);
        var savedArtifact = resources.DbContext.Artifacts.Where(r => r.ArtifactName == artifactName).ToList();
        Assert.AreEqual(1, savedArtifact.Count);
        Assert.IsTrue(savedArtifact.Any(a => a.ArtifactId == existingArtifact.ArtifactId));
        Assert.IsTrue(savedArtifact.Any(a => a.ArtifactId == existingArtifact.ArtifactId));
      }
    }

    [TestMethod]
    public void EditArtifact_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var requestModel = new EditArtifactRequestModel { ArtifactId = artifact.ArtifactId, ArtifactName = artifact.ArtifactName };

        // When
        var result = resources.GuiController.EditArtifact(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedArtifact = resources.DbContext.Artifacts.First(r => r.ArtifactName == requestModel.ArtifactName);
        Assert.AreEqual(requestModel.ArtifactId, savedArtifact.ArtifactId);
      }
    }

    [TestMethod]
    public void RemoveArtifact()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var requestModel = new RemoveArtifactRequestModel { ArtifactId = artifact.ArtifactId };

        // When
        resources.GuiController.RemoveArtifact(requestModel);

        // Then
        var savedArtifact = resources.DbContext.Artifacts.FirstOrDefault(r => r.ArtifactId == requestModel.ArtifactId);
        Assert.IsNull(savedArtifact);
      }
    }

    [TestMethod]
    public void GetAll_PermissionsList()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.DbContext);
        var roleOne = TestHelper.CreateRole(resources.DbContext);
        var roleTwo = TestHelper.CreateRole(resources.DbContext);
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var permissionOne = TestHelper.CreatePermission(resources.DbContext, artifact, roleOne.RoleId);
        var permissionTwo = TestHelper.CreatePermission(resources.DbContext, artifact, roleTwo.RoleId);

        var requestModel = new FindPermissionsRequestModel { ArtifactId = artifact.ArtifactId };

        // When
        var result = resources.GuiController.GetAllPermissions(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        // validate Role Names of the resultset
        Assert.IsTrue(result.Any(r => r.RoleName == roleOne.RoleName));
        Assert.IsTrue(result.Any(r => r.RoleName == roleTwo.RoleName));

        // validate permissionId
        Assert.IsTrue(result.Any(r => r.PermissionId == permissionOne.PermissionId));
        Assert.IsTrue(result.Any(r => r.PermissionId == permissionTwo.PermissionId));

        Assert.IsTrue(result.Any(r => r.ArtifactId == permissionOne.ArtifactId));
        Assert.IsTrue(result.Any(r => r.ArtifactId == permissionTwo.ArtifactId));
      }
    }

    [TestMethod]
    public void AddPermission_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.DbContext);
        var roleOne = TestHelper.CreateRole(resources.DbContext);
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var requestModel = new AddPermissionRequestModel { RoleId = roleOne.RoleId, ArtifactId = artifact.ArtifactId, CreateUser = "testuser"};

        // Whens
        var result = resources.GuiController.AddPermission(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedArtifact = resources.DbContext.Artifacts.First(a => a.ArtifactId == artifact.ArtifactId);

        Assert.AreEqual(1, savedArtifact.Permissions.Count);
        Assert.IsTrue(savedArtifact.Permissions.Any(r => r.RoleId == requestModel.RoleId));
        Assert.IsTrue(savedArtifact.Permissions.Any(r => r.ArtifactId == requestModel.ArtifactId));
      }
    }

    [TestMethod]
    public void AddPermission_ValidationResult_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.DbContext);
        var roleOne = TestHelper.CreateRole(resources.DbContext);
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var permission = TestHelper.CreatePermission(resources.DbContext, artifact, roleOne.RoleId);
        var requestModel = new AddPermissionRequestModel { RoleId = roleOne.RoleId, ArtifactId = artifact.ArtifactId };

        // When
        var result = resources.GuiController.AddPermission(requestModel);

        // Then
        Assert.IsFalse(result.IsValid);

        var savedRPermissions = resources.DbContext.Artifacts.First(r => r.ArtifactId == requestModel.ArtifactId).Permissions;

        Assert.AreEqual(1, savedRPermissions.Count);
        Assert.IsTrue(savedRPermissions.Any(r => r.RoleId == roleOne.RoleId));
        Assert.IsTrue(savedRPermissions.Any(r => r.ArtifactId == artifact.ArtifactId));
      }
    }

    [TestMethod]
    public void RemovePermission()
    {
      var resources = new Resources();

      using (resources.DbContext.Database.BeginTransaction())
      {
        // Given
        var roleOne = TestHelper.CreateRole(resources.DbContext);
        var artifact = TestHelper.CreateArtifact(resources.DbContext);
        var permission = TestHelper.CreatePermission(resources.DbContext, artifact, roleOne.RoleId);
        var requestModel = new RemovePermissionRequestModel { PermissionId = permission.PermissionId, ArtifactId = artifact.ArtifactId };

        // When
        resources.GuiController.RemovePermission(requestModel);

        // Then
        var savedPermission = resources.DbContext.Artifacts.FirstOrDefault(r => r.ArtifactId == artifact.ArtifactId).Permissions.FirstOrDefault(p => p.PermissionId == requestModel.PermissionId);
        Assert.IsNull(savedPermission);
      }
    }
  }
}
