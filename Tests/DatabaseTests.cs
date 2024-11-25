using PhoenixModel.Program;
using PhoenixWPF.Database;

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
                ShowWindowDiplomaty = false
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
            Assert.Equal(userSettings.ShowWindowDiplomaty, loadedSettings.ShowWindowDiplomaty);
        }

        [Fact]
        public void AppSettings()
        {
            // Arrange
            var settings1 = new AppSettings();

            settings1.InitializeSettings("Test.jpk");
            settings1.UserSettings.DatabaseLocationKarte = "ABC";

            var settings2 = new AppSettings();
            settings2.InitializeSettings("Test.jpk");

            // Assert
            Assert.Equal(settings1.UserSettings.DatabaseLocationKarte, settings2.UserSettings.DatabaseLocationKarte);
            Assert.Equal(settings1.UserSettings.ShowWindowNavigator, settings2.UserSettings.ShowWindowNavigator);
            Assert.Equal(settings1.UserSettings.ShowWindowProperties, settings2.UserSettings.ShowWindowProperties);
            Assert.Equal(settings1.UserSettings.ShowWindowDiplomaty, settings2.UserSettings.ShowWindowDiplomaty);
        }
    }
}