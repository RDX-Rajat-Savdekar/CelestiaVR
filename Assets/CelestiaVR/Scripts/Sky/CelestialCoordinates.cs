// CelestialCoordinates.cs
// Reference RA/Dec values for CelestiaVR's celestial objects.
// Planet positions are approximate for late December 2025 (a good winter night from LA).
// Fixed stars/DSOs are J2000 catalog positions (essentially permanent on human timescales).
//
// Use these values when creating CelestialObjectData ScriptableAssets in the Editor.
//
// RA format: decimal hours (0–24)
// Dec format: decimal degrees (-90 to +90)
//
// Object              RA (h)     Dec (°)    Notes
// ─────────────────────────────────────────────────────────────
// Jupiter             ~5.30      ~22.5      Dec 2025 approx (Taurus)
// Saturn              ~22.85     ~-13.5     Dec 2025 approx (Aquarius)
// Mars                ~6.90      ~26.0      Dec 2025 approx (Gemini/Taurus)
// Venus               ~20.50     ~-24.0     Dec 2025 approx (Sagittarius, evening)
// Mercury             ~19.80     ~-22.0     Dec 2025 approx (Sagittarius, near Venus)
// Sirius (α CMa)       6.7525    -16.7161   J2000 catalog
// Betelgeuse (α Ori)   5.9194     7.4070    J2000 catalog
// Orion Nebula (M42)   5.5833    -5.3911    J2000 catalog
// Andromeda (M31)      0.7122    41.2689    J2000 catalog
// Pleiades (M45)       3.7908    24.1050    J2000 catalog
//
// IMPORTANT: Planet RAs/Decs should be verified via NASA JPL Horizons for the
// exact experience date. This file is a starting reference only.
//
// How to use SkyDomeController.SetInitialRotation():
//   The initial rotation offset aligns the vernal equinox (RA=0h) with its correct
//   azimuth for the chosen local sidereal time (LST) at Griffith Observatory.
//   LST = GMST + longitude_correction. For a Dec 21 2025 9pm PST from Griffith:
//   LST ≈ 3.5h → initial rotation = -(LST * 15) degrees around the pole axis.
//   In practice, adjust this in the Editor until Saturn/Jupiter look right.

// This file is documentation-only. No runtime code.
