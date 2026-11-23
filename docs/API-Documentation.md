# External API: Open-Meteo Forecast

## Purpose and user value

The dashboard gets a seven-day forecast for the selected pantry site. Coordinators can use precipitation probability and severe-weather codes to decide whether to move an outdoor pickup indoors or change volunteer coverage. If the provider is unavailable, the dashboard shows a clear weather status while normal pantry functions continue.

## Provider and access

- Provider: [Open-Meteo Forecast API](https://open-meteo.com/en/docs)
- Endpoint: `GET https://api.open-meteo.com/v1/forecast`
- Authentication: no API key for this classroom demonstration; follow provider terms and applicable usage limits.
- Request fields: `latitude`, `longitude`, `daily=weather_code,temperature_2m_max,precipitation_probability_max`, `forecast_days=7`, `timezone=auto`.
- Coordinates come from the pantry site record; the app does not send household or donor information.

## Example

```text
GET /v1/forecast?latitude=39.7817&longitude=-89.6501
    &daily=weather_code,temperature_2m_max,precipitation_probability_max
    &forecast_days=7&timezone=auto
```

The provider returns daily dates, maximum temperature, precipitation probability, and WMO weather codes. The app displays date, high temperature, and rain chance. A rain probability of 60% or more, or a thunderstorm code (95, 96, 99), produces an operations advisory.

## Failure behavior

The request has an eight-second timeout. Connection errors and timeouts are logged and surfaced as an availability message on the dashboard; they do not prevent inventory, household, donation, or distribution workflows. Provider schema validation guards against missing daily arrays.

## Test considerations

- Confirm that a configured pantry coordinate returns forecast rows.
- Test malformed/incomplete responses and unavailable network behavior.
- Confirm forecast location follows the selected pantry site.
- Treat weather as planning guidance, not a safety alert or guarantee.
