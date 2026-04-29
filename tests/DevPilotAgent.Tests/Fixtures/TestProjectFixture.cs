namespace DevPilotAgent.Tests.Fixtures;

public class TestProjectFixture : IDisposable
{
    public string TestFolderPath { get; }

    public TestProjectFixture()
    {
        TestFolderPath = Path.Combine(Path.GetTempPath(), $"devpilot_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TestFolderPath);
        SetupTestFiles();
    }

    private void SetupTestFiles()
    {
        Directory.CreateDirectory(Path.Combine(TestFolderPath, "Services"));
        Directory.CreateDirectory(Path.Combine(TestFolderPath, "Models"));
        Directory.CreateDirectory(Path.Combine(TestFolderPath, "bin"));

        File.WriteAllText(Path.Combine(TestFolderPath, "Services", "UserService.cs"),
            """
            namespace MyApp.Services;
            public class UserService
            {
                public User GetUser(int id)
                {
                    var user = _repository.Find(id);
                    return user.Name.ToUpper(); // NullReferenceException 가능
                }
            }
            """);

        File.WriteAllText(Path.Combine(TestFolderPath, "Models", "User.cs"),
            """
            namespace MyApp.Models;
            public class User
            {
                public int Id { get; set; }
                public string? Name { get; set; }
            }
            """);

        File.WriteAllText(Path.Combine(TestFolderPath, "bin", "should_be_excluded.cs"),
            "// 이 파일은 bin 폴더에 있으므로 검색에서 제외되어야 함");
    }

    public void Dispose()
    {
        try { Directory.Delete(TestFolderPath, recursive: true); }
        catch { /* cleanup best effort */ }
    }
}
