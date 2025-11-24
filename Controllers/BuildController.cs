using Microsoft.AspNetCore.Mvc;

namespace BuildToolService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildController : ControllerBase
    {
        private readonly IBuildToolService _buildToolService;

        public BuildController(IBuildToolService buildToolService)
        {
            _buildToolService = buildToolService;
        }

        [HttpGet("auslieferung")]
        public IActionResult Auslieferung(
            [FromQuery] string auslieferung,
            [FromQuery(Name = "version_von")] string versionVon,
            [FromQuery(Name = "version_bis")] string versionBis,
            [FromQuery(Name = "fix_von")] string fixVon,
            [FromQuery(Name = "fix_bis")] string fixBis,
            [FromQuery] string kunde,
            [FromQuery(Name = "neue_todo")] string neueTodo,
            [FromQuery(Name = "ef_online_ordner_erstellen")] string ef3OrdnerErstellen,
            [FromQuery(Name = "installationsroutine_erstellen")] string installationsroutineErstellen,
            [FromQuery(Name = "unterfixe_beruecksichtigen")] string unterfixeBeruecksichtigen,
            [FromQuery(Name = "auslieferung_lokal_erstellen")] string lokalErstellen,
            [FromQuery(Name = "woechentlichen_build")] string woechentlichenBuild,
            [FromQuery(Name = "32bit")] string bit32,
            [FromQuery] string? internAuslieferung)
        {
            var result = _buildToolService.AuslieferungErstellen(auslieferung, versionVon, versionBis, fixVon, fixBis, kunde, neueTodo, ef3OrdnerErstellen, installationsroutineErstellen, unterfixeBeruecksichtigen, lokalErstellen, woechentlichenBuild, bit32, internAuslieferung);
            return Ok(result);
        }

        [HttpGet("grossmigration")]
        public IActionResult GrossMigration([FromQuery(Name = "version_von")] string versionVon, [FromQuery(Name = "version_bis")] string versionBis, [FromQuery] string kunde)
        {
            var result = _buildToolService.GrossMigrationErstellen(versionVon, versionBis, kunde);
            return Ok(result);
        }

        [HttpGet("migrationsordner")]
        public IActionResult Migrationsordner([FromQuery(Name = "version_von")] string versionVon, [FromQuery(Name = "version_bis")] string versionBis, [FromQuery(Name = "datum_von")] string datumVon, [FromQuery(Name = "datum_bis")] string datumBis)
        {
            var result = _buildToolService.MigrationsordnerZusammenErstellen(versionVon, versionBis, datumVon, datumBis);
            return Ok(result);
        }

        [HttpGet("migrationdurchfuehren")]
        public IActionResult MigrationDurchfuehren([FromQuery(Name = "version_von")] string versionVon, [FromQuery(Name = "version_bis")] string versionBis, [FromQuery] string kunde)
        {
            var result = _buildToolService.DatenbankenMigrieren(versionVon, versionBis, kunde);
            return Ok(result);
        }

        [HttpGet("testmigrationdurchfuehren")]
        public IActionResult TestMigration([FromQuery(Name = "version_von")] string versionVon, [FromQuery(Name = "version_bis")] string versionBis)
        {
            var result = _buildToolService.TestDatenbankMigrieren(versionVon, versionBis);
            return Ok(result);
        }

        [HttpGet("versionersetzen")]
        public IActionResult Versionersetzen([FromQuery(Name = "version")] string version, [FromQuery(Name = "kunde")] string kunde)
        {
            var result = _buildToolService.VersionFuerEfErsetzen(version, kunde);
            return Ok(result);
        }

        [HttpGet("get_kundenliste")]
        public IActionResult KundenListe()
        {
            return Ok(_buildToolService.GetKundeListe());
        }

        [HttpGet("get_kundetestdatenbankliste")]
        public IActionResult KundenTestDb()
        {
            return Ok(_buildToolService.GetKundeTestDatenbankListe());
        }

        [HttpGet("get_kdatenbank")]
        public IActionResult KDatenbank()
        {
            return Ok(_buildToolService.GetKDatenbank());
        }

        [HttpGet("get_kserver")]
        public IActionResult KServer()
        {
            return Ok(_buildToolService.GetKServer());
        }

        [HttpGet("get_kkuerzel")]
        public IActionResult KKuerzel()
        {
            return Ok(_buildToolService.GetKKuerzel());
        }
    }
}
