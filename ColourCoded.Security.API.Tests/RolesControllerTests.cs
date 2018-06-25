using Microsoft.VisualStudio.TestTools.UnitTesting;
using ColourCoded.Security.API.Controllers;
using ColourCoded.Security.API.Data;
using Microsoft.Extensions.Configuration;
using ColourCoded.Security.API.Models.RequestModels.Role;
using System.Linq;

namespace ColourCoded.Security.API.Tests
{
  [TestClass]
  public class RolesControllerTests
  {
    private class Resources
    {
      public RolesController Controller;
      public SecurityContext Context;
      public IConfiguration Configuration; 

      public Resources()
      {
        Configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

        Context = TestHelper.CreateDbContext(Configuration);
        Controller = new RolesController(Context);
      }
    }

    [TestMethod]
    public void GetAll_RolesList()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.Context);
        var role = TestHelper.CreateRole(resources.Context);

        // When
        var result = resources.Controller.GetAll();

        // Then
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.Any(r => r.RoleId == role.RoleId));
        Assert.IsTrue(result.Any(r => r.RoleName == role.RoleName));
      }
    }

    [TestMethod]
    public void SearchUsers()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.CreateUser(resources.Context, username: "testuser1");
        TestHelper.CreateUser(resources.Context, username: "testuser2");
        TestHelper.CreateUser(resources.Context, username: "ohno");
        TestHelper.CreateUser(resources.Context, username: "jumptester");

        // When
        var result = resources.Controller.SearchUsers(new SearchUsersRequestModel { SearchTerm = "test"});

        // Then
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);;
      }
    }

    [TestMethod]
    public void AddRole_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var requestModel = new AddRoleRequestModel { RoleName = "Test Role" , CreateUser = "testuser"};

        // When
        var result = resources.Controller.AddRole(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedRole = resources.Context.Roles.First(r => r.RoleName == requestModel.RoleName);
      }
    }

    [TestMethod]
    public void AddRole_ValidationResult_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        const string roleName = "Test Role";
        TestHelper.CreateRole(resources.Context, roleName);
        var requestModel = new AddRoleRequestModel { RoleName = roleName };

        // When
        var result = resources.Controller.AddRole(requestModel);

        // Then
        Assert.IsFalse(result.IsValid);

        var savedRole = resources.Context.Roles.Count(r => r.RoleName == roleName);
        Assert.AreEqual(1, savedRole);
      }
    }

    [TestMethod]
    public void EditRole_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var role = TestHelper.CreateRole(resources.Context);
        var requestModel = new EditRoleRequestModel { RoleId = role.RoleId, RoleName = role.RoleName };

        // When
        var result = resources.Controller.EditRole(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedRole = resources.Context.Roles.First(r => r.RoleName == requestModel.RoleName);
        Assert.AreEqual(requestModel.RoleId, savedRole.RoleId);
      }
    }

    [TestMethod]
    public void RemoveRole()
    {
      var resources = new Resources();

      using(resources.Context.Database.BeginTransaction())
      {
        // Given
        var role = TestHelper.CreateRole(resources.Context);
        var requestModel = new RemoveRoleRequestModel { RoleId = role.RoleId };

        // When
        resources.Controller.RemoveRole(requestModel);

        // Then
        var savedRole = resources.Context.Roles.FirstOrDefault(r => r.RoleId == requestModel.RoleId);
        Assert.IsNull(savedRole);
      }
    }

    [TestMethod]
    public void GetAll_RoleMembersList()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.Context);
        var role = TestHelper.CreateRole(resources.Context);
        var roleMemberOne = TestHelper.CreateRoleMember(resources.Context, role, "Test RoleMemberOne");
        var roleMemberTwo = TestHelper.CreateRoleMember(resources.Context, role, "Test RoleMemberTwo");

        var requestModel = new FindRoleMembersRequestModel { RoleId = role.RoleId };

        // When
        var result = resources.Controller.GetAllMembers(requestModel);

        // Then
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(r => r.Username == roleMemberOne.Username));
        Assert.IsTrue(result.Any(r => r.Username == roleMemberTwo.Username));
        Assert.IsTrue(result.Any(r => r.RoleMemberId == roleMemberOne.RoleMemberId));
        Assert.IsTrue(result.Any(r => r.RoleMemberId == roleMemberTwo.RoleMemberId));
        Assert.IsTrue(result.Any(r => r.RoleId == roleMemberOne.RoleId));
        Assert.IsTrue(result.Any(r => r.RoleId == roleMemberTwo.RoleId));
      }
    }

    [TestMethod]
    public void AddRoleMember_ValidationResult_Success()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.Context);
        var role = TestHelper.CreateRole(resources.Context);
        var requestModel = new AddRoleMemberRequestModel { RoleId = role.RoleId, Username = "testuser", CreateUser = "sa" };

        // When
        var result = resources.Controller.AddRoleMember(requestModel);

        // Then
        Assert.IsTrue(result.IsValid);

        var savedRole = resources.Context.Roles.First(r => r.RoleId == requestModel.RoleId);
        Assert.AreEqual(1, savedRole.RoleMembers.Count);
        Assert.IsTrue(savedRole.RoleMembers.Any(r => r.RoleId == role.RoleId));
        Assert.IsTrue(savedRole.RoleMembers.Any(r => r.Username == requestModel.Username));
      }
    }

    [TestMethod]
    public void AddRoleMember_ValidationResult_AlreadyExists()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        TestHelper.RemoveRoles(resources.Context);
        var role = TestHelper.CreateRole(resources.Context);
        var roleMember = TestHelper.CreateRoleMember(resources.Context, role, "testuser");
        var requestModel = new AddRoleMemberRequestModel { RoleId = role.RoleId, Username = roleMember.Username, CreateUser = "sa" };

        // When
        var result = resources.Controller.AddRoleMember(requestModel);

        // Then
        Assert.IsFalse(result.IsValid);

        var savedRolemembers = resources.Context.Roles.First(r => r.RoleId == requestModel.RoleId).RoleMembers;

        Assert.AreEqual(1, savedRolemembers.Count);
        Assert.IsTrue(savedRolemembers.Any(r => r.RoleId == roleMember.RoleId));
        Assert.IsTrue(savedRolemembers.Any(r => r.Username == roleMember.Username));
      }
    }

    [TestMethod]
    public void RemoveRoleMember()
    {
      var resources = new Resources();

      using (resources.Context.Database.BeginTransaction())
      {
        // Given
        var role = TestHelper.CreateRole(resources.Context);
        var roleMember = TestHelper.CreateRoleMember(resources.Context, role, "testuser");
        var requestModel = new RemoveRoleMemberRequestModel { RoleId = role.RoleId, RoleMemberId = roleMember.RoleMemberId };

        // When
        resources.Controller.RemoveRoleMember(requestModel);

        // Then
        var savedRole = resources.Context.Roles.FirstOrDefault(r => r.RoleId == requestModel.RoleId);
        Assert.IsNull(savedRole.RoleMembers.FirstOrDefault(rm => rm.RoleMemberId == requestModel.RoleMemberId));
      }
    }
  }
}
