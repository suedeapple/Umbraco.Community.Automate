using Umbraco.Automate.Core.Settings;
using Umbraco.Community.Automate.Skoda.Models;

namespace Umbraco.Community.Automate.Skoda.Actions;

public sealed class UpdateChargingProfileSettings
{
    [Field(
        Label = "Charging profile ID",
        Description = "Identifier of the charging profile to update, as returned by Get Vehicle Status.")]
    public long ProfileId { get; set; }

    [Field(Label = "Name", Description = "Leave empty to keep the profile's current name.", SupportsBindings = true)]
    public string? Name { get; set; }

    [Field(
        Label = "Target state of charge",
        Description = "Target state of charge in percent. Leave empty to keep the profile's current value.")]
    public int? TargetStateOfChargeInPercent { get; set; }

    [Field(
        Label = "Max charging current (AC)",
        EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
        EditorConfig = """[{ "alias": "items", "value": [{ "name": "Keep current setting", "value": "" }, { "name": "Reduced", "value": "REDUCED" }, { "name": "Maximum", "value": "MAXIMUM" }] }]""")]
    public string? MaxChargingCurrent { get; set; }

    [Field(
        Label = "Auto unlock plug when charged",
        EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
        EditorConfig = """[{ "alias": "items", "value": [{ "name": "Keep current setting", "value": "" }, { "name": "Permanently", "value": "PERMANENT" }, { "name": "Off", "value": "OFF" }] }]""")]
    public string? AutoUnlockPlugWhenCharged { get; set; }
}
