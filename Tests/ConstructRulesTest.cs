using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Tests {
    public class ConstructRulesTest {
        // 310/128 Wasser / Küste
        KleinFeld? wasser;

        // 301/44 Wald/ Helborn
        KleinFeld? waldHelborn;

        // 204/48 Tiefland Theostelos
        KleinFeld? tieflandTheostelos;

        // 305/10 Wald / Theostelos Küste NE + E
        KleinFeld? waldTheostelosKüsteNEE;

        // 204/37 Festung / Tiefland / Theostelos Küste NW
        KleinFeld? Nowidat;


        private void IsNotAllowedOnWater_ShouldReturnError_WhenKleinFeldIsWasser() {
            // Act
            var result = ConstructRules.IsNotAllowedOnWater(wasser);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            // Act
            result = ConstructRules.IsNotAllowedOnWater(tieflandTheostelos);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
        }

        private void IsAllowedForOwner_ShouldReturnError_WhenKleinFeldDoesNotBelongToUser() {
            // Act
            var result = ConstructRules.IsAllowedForOwner(waldHelborn);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            // Act
            result = ConstructRules.IsAllowedForOwner(tieflandTheostelos);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
        }

        private void IsRüstPhase_ShouldReturnError_WhenNotConstructionPhase() {
            Assert.NotNull(SharedData.ZugdatenSettings);

            SharedData.ZugdatenSettings.Last().Phase = 01;
            // Act
            var result = ConstructRules.IsRüstPhase();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            SharedData.ZugdatenSettings.Last().Phase = 0;
            // Act
            result = ConstructRules.IsRüstPhase();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
        }

        private void CanConstructCastle() {
            // not owner
            var result = ConstructRules.CanConstructCastle(waldHelborn);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            var schatzkammer = SchatzkammerView.GetActual();
            schatzkammer.Reichschatz = 100;

            // zu wenig Geld
            result = ConstructRules.CanConstructCastle(waldTheostelosKüsteNEE);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            schatzkammer.Reichschatz = 100000;
            
            // schon besetzt
            result = ConstructRules.CanConstructCastle(Nowidat);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            // jetzt aber alles gut
            result = ConstructRules.CanConstructCastle(waldTheostelosKüsteNEE);
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
        }

        private void CanConstructWall() {
            // not owner
            var result = ConstructRules.CanConstructWall(waldHelborn, Direction.SW);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            var schatzkammer = SchatzkammerView.GetActual();
            schatzkammer.Reichschatz = 100;

            // zu wenig Geld
            result = ConstructRules.CanConstructWall(waldTheostelosKüsteNEE, Direction.NW);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            schatzkammer.Reichschatz = 100000;

            // schon besetzt
            result = ConstructRules.CanConstructWall(Nowidat, Direction.SO);
            Assert.NotNull(result);
            Assert.True(result.HasErrors);

            // jetzt aber alles gut
            result = ConstructRules.CanConstructWall(waldTheostelosKüsteNEE, Direction.NW);
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
        }


        [StaFact]
        public void ConstructRulesBasics() {

            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);

            Assert.NotNull(SharedData.Map);

            // 201/27 Wasser / Küste
            wasser = SharedData.Map["201/27"];

            // 301/44 Wald/ Helborn
            waldHelborn = SharedData.Map["301/44"];

            // 204/48 Tiefland Theostelos
            tieflandTheostelos = SharedData.Map["204/48"];

            // 305/10 Wald / Theostelos Küste NE + E
            waldTheostelosKüsteNEE = SharedData.Map["305/10"];

            // 406/17 Festung / Tiefland / Theostelos Küste NW
            Nowidat = SharedData.Map["204/37"];

            IsNotAllowedOnWater_ShouldReturnError_WhenKleinFeldIsWasser();
            IsAllowedForOwner_ShouldReturnError_WhenKleinFeldDoesNotBelongToUser();
            IsRüstPhase_ShouldReturnError_WhenNotConstructionPhase();
            CanConstructCastle();
            CanConstructWall();
        }
    }
}
