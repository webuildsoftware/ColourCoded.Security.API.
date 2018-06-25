using Microsoft.EntityFrameworkCore;
using ColourCoded.Security.API.Data;
using Microsoft.Extensions.Configuration;
using ColourCoded.Security.API.Data.Entities.Role;
using ColourCoded.Security.API.Data.Entities.Permission;
using System;
using ColourCoded.Security.API.Data.Entities.Login;

namespace ColourCoded.Security.API.Tests
{
  public static class TestHelper
  {
    public static SecurityContext CreateDbContext(IConfiguration configuration)
    {
      var optionsBuilder = new DbContextOptionsBuilder<SecurityContext>();
      optionsBuilder.UseSqlServer(configuration.GetConnectionString("ColourCoded_Security_OLTP"));

      return new SecurityContext(optionsBuilder.Options);
    }

    public static void RemoveRoles(SecurityContext context)
    {
      context.Database.ExecuteSqlCommand("truncate table Roles");
      context.Database.ExecuteSqlCommand("truncate table RoleMembers");
    }

    public static Role CreateRole(SecurityContext context, string roleName = "Test Role")
    {
      var role = new Role { RoleName = roleName, CreateDate = DateTime.Now, CreateUser = "sa" };
      context.Roles.Add(role);
      context.SaveChanges();

      return role;
    }

    public static void RemoveArtifacts(SecurityContext context)
    {
      context.Database.ExecuteSqlCommand("truncate table Artifacts");
      context.Database.ExecuteSqlCommand("truncate table Permissions");
    }

    public static RoleMember CreateRoleMember(SecurityContext context, Role role, string username)
    {
      var roleMember = new RoleMember { Username = username, RoleId = role.RoleId, CreateDate = DateTime.Now, CreateUser = "sa" };
      role.RoleMembers.Add(roleMember);

      context.SaveChanges();

      return roleMember;
    }


    public static Artifact CreateArtifact(SecurityContext context, string artifactName = "Test Artifact")
    {
      var artifact = new Artifact { ArtifactName = artifactName, CreateDate = DateTime.Now, CreateUser = "sa" };
      context.Artifacts.Add(artifact);
      context.SaveChanges();

      return artifact;
    }

    public static Permission CreatePermission(SecurityContext context, Artifact artifact, int roleId)
    {
      var permission = new Permission { ArtifactId = artifact.ArtifactId, RoleId = roleId, CreateDate = DateTime.Now, CreateUser = "sa" };
      artifact.Permissions.Add(permission);

      context.SaveChanges();

      return permission;
    }

    public static void RemoveUsers(SecurityContext context)
    {
      context.Database.ExecuteSqlCommand("truncate table Users");
    }

    public static User CreateUser(SecurityContext context, string username = "testuser", string password = "password", string emailAddress = "test@gmail.com")
    {
      var user = new User
      {
        Username = username,
        Password = password,
        FirstName = "Test",
        LastName = "User",
        EmailAddress = emailAddress,
        CreateDate = DateTime.Now,
        CreateUser = "sa"
      };

      context.Users.Add(user);
      context.SaveChanges();

      return user;
    }
  }
}
