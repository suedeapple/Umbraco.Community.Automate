# Umbraco.Community.Automate.Skoda

A Škoda connection type and actions for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate), built on the MyŠkoda Public API.

Read a vehicle's status and control charging, air conditioning, auxiliary heating and ventilation as part of an automation workflow.

> **In development.** This package hasn't been released to NuGet yet.

## Installation

```bash
dotnet add package Umbraco.Community.Automate.Skoda
```

No further setup required. The composer registers itself automatically via Umbraco's `IComposer` discovery.

## Setup

### 1. Get an API key

Generate an API key in the **MyŠkoda** app. You'll also need the vehicle's VIN (Vehicle Identification Number).

### 2. Store the key in configuration

Add the key under `Umbraco:Automate:Secrets:Skoda`, in Umbraco Automate's shared Secrets section (locally, use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):

```json
{
  "Umbraco": {
    "Automate": {
      "Secrets": {
        "Skoda": {
          "ApiKey": "your-api-key"
        }
      }
    }
  }
}
```

In production, use an environment variable instead: `Umbraco__Automate__Secrets__Skoda__ApiKey=your-api-key`. Umbraco Automate resolves references from this shared section out of the box, so there's nothing to register.

### 3. Create the connection

1. Go to **Automation → Settings → Connections** and create a new **Škoda** connection.
2. **API key** is already filled in with `$Umbraco:Automate:Secrets:Skoda:ApiKey`, a reference to the key you stored in step 2. Leave it, or replace it with the key itself to store it on the connection.
3. **VIN**: the vehicle you want to control. Create one connection per vehicle.
4. **Validate connection**: when on, saving or testing the connection checks the API key and VIN against the Škoda API. Each check uses one request from your API quota, so it's off by default.

## Actions

All actions are in the **Skoda** group and act on the vehicle set on the connection.

| Action | Settings |
|---|---|
| **Get Vehicle Status** | None. Returns the vehicle's details and current state (see [Outputs](#outputs)). |
| **Start Charging** / **Stop Charging** | None. |
| **Set Charge Mode** | **Charge mode**: `MANUAL`, `TIMER`, `TIMER_CHARGING_WITH_CLIMATISATION`, `PREFERRED_CHARGING_TIMES`, `ONLY_OWN_CURRENT`, `IMMEDIATE_DISCHARGING` or `HOME_STORAGE_CHARGING`. |
| **Set Charging Limit** | **Target state of charge** in percent. Vehicles typically accept 50–100 in steps of 10. |
| **Update Charging Profile** | **Charging profile ID** (from *Get Vehicle Status*), plus any of **Name**, **Target state of charge**, **Max charging current (AC)** (`REDUCED` or `MAXIMUM`) and **Auto unlock plug when charged** (`PERMANENT` or `OFF`). Empty fields keep their current value. |
| **Start Air Conditioning** | **Target temperature**, **Temperature unit** (`CELSIUS` or `FAHRENHEIT`), **Allow without external power**. |
| **Stop Air Conditioning** | None. |
| **Start Auxiliary Heating** | **Target temperature**, **Temperature unit**, **S-PIN** (required), **Duration in seconds** (default 1800), **Start mode** (`HEATING` or `VENTILATION`). |
| **Stop Auxiliary Heating** | None. |
| **Start Active Ventilation** / **Stop Active Ventilation** | None. |

Settings support `${ binding }` expressions, so values can come from the trigger or earlier steps.

### Outputs

**Get Vehicle Status** makes these available to later steps as `${ steps.<alias>.<field> }`:

| Field | Description |
|---|---|
| `vin`, `name`, `licensePlate` | Vehicle details |
| `mileageInKm` | Odometer reading |
| `stateOfChargeInPercent`, `remainingCruisingRangeInMeters` | Battery and range |
| `chargingState`, `airConditioningState`, `parkingState` | Current states |
| `latitude`, `longitude`, `formattedAddress` | Parked position |
| `errors` | Any errors the vehicle reported (`type`, `description`) |

The command actions output the `vin` they acted on.

## Links

- [Source code](https://github.com/umbraco-community/Umbraco.Community.Automate/tree/main/Connections/Skoda/Umbraco.Community.Automate.Skoda)
- [Report an issue](https://github.com/umbraco-community/Umbraco.Community.Automate/issues)
