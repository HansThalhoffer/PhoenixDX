using PhoenixModel.Database;
using PhoenixModel.Program;
using PhoenixModel.dbErkenfara;
using PhoenixWPF.Database;
using PhoenixWPF.Program;
using PhoenixWPF.Dialogs;

using static PhoenixModel.Database.PasswordHolder;
using PhoenixWPF.Helper;
using System.Windows.Controls;
using System.Windows;
using PhoenixModel.ViewModel;

namespace Tests
{
    public class Database
    {
        [Fact]
        public void ObjectStore_CanStoreAndRetrieve_UserSettings()
        {
            // Arrange
            var store = new ObjectStore();
            var userSettings = new UserSettings
            {
                DatabaseLocationKarte = "_Data\\Database\\CustomDB.mdb",
                ShowWindowNavigator = false,
                ShowWindowProperties = true,
                ShowWindowDiplomacy = false
            };

            // Act
            store.Add(userSettings);

            // Serialize the store to a JSON string
            string jsonString = store.Serialize();

            // Deserialize the store from the JSON string
            var newStore = new ObjectStore();
            newStore.Deserialize(jsonString);

            // Retrieve the UserSettings from the new store
            var loadedSettings = newStore.Get<UserSettings>();

            // Assert
            Assert.NotNull(loadedSettings);
            Assert.Equal(userSettings.DatabaseLocationKarte, loadedSettings.DatabaseLocationKarte);
            Assert.Equal(userSettings.ShowWindowNavigator, loadedSettings.ShowWindowNavigator);
            Assert.Equal(userSettings.ShowWindowProperties, loadedSettings.ShowWindowProperties);
            Assert.Equal(userSettings.ShowWindowDiplomacy, loadedSettings.ShowWindowDiplomacy);
        }
        
        [StaFact]
        [STAThread]
        public void LoadKarteTest()
        {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            Assert.NotNull(SharedData.Map);
            Assert.NotEmpty(SharedData.Map);
        }

        [StaFact]
        [STAThread]
        public void LoadPZETest() {
            TestSetup.Setup();
            TestSetup.LoadPZE(false, false);
            Assert.NotNull(SharedData.Nationen);
            Assert.NotEmpty(SharedData.Nationen);
        }

        [StaFact]
        [STAThread]
        public void LoadCrossRefTest() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            Assert.NotNull(SharedData.Kosten);
            Assert.NotEmpty(SharedData.Kosten);
        }


        [StaFact]
        public void EncryptDecryptPassword_ShouldReturnOriginalPassword()
        {
           // Arrange
            string expected = "MySecurePassword123!";
            PasswordHolder pwdHolder1 = new PasswordHolder(expected);
            var encrypted = pwdHolder1.EncryptedPasswordBase64;

            PasswordHolder pwdHolder2 = new PasswordHolder(encrypted);
            string? actual  = pwdHolder2.DecryptedPassword;

            // Assert
            Assert.Equal(expected, actual);

            string json = System.Text.Json.JsonSerializer.Serialize(pwdHolder2);
            PasswordHolder? deserializedPwdHolder = System.Text.Json.JsonSerializer.Deserialize<PasswordHolder>(json);

            actual = deserializedPwdHolder?.DecryptedPassword;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AppSettingsTest()
        {
            // Arrange
            var settings1 = new AppSettings("Test_settings1.jpk");

            settings1.InitializeSettings();
            settings1.UserSettings.DatabaseLocationKarte = "ABC";

            var settings2 = new AppSettings("Test_settings1.jpk");
            settings2.InitializeSettings();

            // Assert
            Assert.Equal(settings1.UserSettings.DatabaseLocationKarte, settings2.UserSettings.DatabaseLocationKarte);
            Assert.Equal(settings1.UserSettings.ShowWindowNavigator, settings2.UserSettings.ShowWindowNavigator);
            Assert.Equal(settings1.UserSettings.ShowWindowProperties, settings2.UserSettings.ShowWindowProperties);
            Assert.Equal(settings1.UserSettings.ShowWindowDiplomacy, settings2.UserSettings.ShowWindowDiplomacy);
        }
    }
}