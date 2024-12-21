using PhoenixModel.Database;
using PhoenixModel.Program;
using PhoenixModel.dbErkenfara;
using PhoenixWPF.Database;
using PhoenixWPF.Program;
using PhoenixWPF.Dialogs;

using static PhoenixModel.Database.PasswordHolder;
using PhoenixWPF.Helper;

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
        
        class PasswortProvider : PasswordHolder.IPasswordProvider
        {
            public EncryptedString Password
            {
                get
                {
                    PasswordDialog dialog = new PasswordDialog("Das Passwort für die UnitTest bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
        }

        [StaFact]
        public void LoadKarte()
        {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();
            settings.UserSettings.DatabaseLocationKarte = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationKarte);

            // Arrange
            PasswordHolder pwdHolder = new PasswordHolder(settings.UserSettings.PasswordKarte, new PasswortProvider());
            settings.UserSettings.PasswordKarte = pwdHolder.EncryptedPasswordBase64;
            string? databasePassword = pwdHolder.DecryptPassword();
            Assert.NotNull(databasePassword);
            Assert.NotEmpty(databasePassword);

            using (ErkenfaraKarte karte = new ErkenfaraKarte(settings.UserSettings.DatabaseLocationKarte, settings.UserSettings.PasswordKarte))
            {
                karte.Load();
               
            }
        }

        [StaFact]
        public void EncryptDecryptPassword_ShouldReturnOriginalPassword()
        {
            // PasswordHolder pwdHolderDialog = new PasswordHolder(null, new PasswortProvider());
          
            // Arrange
            EncryptedString expected = "MySecurePassword123!";
            PasswordHolder pwdHolder = new PasswordHolder(expected);

            string? actual  = pwdHolder.DecryptPassword();

            // Assert
            Assert.Equal(expected, actual);

            string json = System.Text.Json.JsonSerializer.Serialize(pwdHolder);
            PasswordHolder? deserializedPwdHolder = System.Text.Json.JsonSerializer.Deserialize<PasswordHolder>(json);

            actual = deserializedPwdHolder?.DecryptPassword();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AppSettings()
        {
            // Arrange
            var settings1 = new AppSettings("Test.jpk");

            settings1.InitializeSettings();
            settings1.UserSettings.DatabaseLocationKarte = "ABC";

            var settings2 = new AppSettings("Test.jpk");
            settings2.InitializeSettings();

            // Assert
            Assert.Equal(settings1.UserSettings.DatabaseLocationKarte, settings2.UserSettings.DatabaseLocationKarte);
            Assert.Equal(settings1.UserSettings.ShowWindowNavigator, settings2.UserSettings.ShowWindowNavigator);
            Assert.Equal(settings1.UserSettings.ShowWindowProperties, settings2.UserSettings.ShowWindowProperties);
            Assert.Equal(settings1.UserSettings.ShowWindowDiplomacy, settings2.UserSettings.ShowWindowDiplomacy);
        }
    }
}