# 🚗 Bit Shox - Unity Vehicle Simulation

A realistic vehicle simulation system in Unity. Bit Shox was developed in Unity 2022.3, and has not been tested in more recent versions of Unity (ex. 2023.x, 6000.x).

## ⚙️ Startup Guide

Getting a working vehicle is super easy! Simply drag a prefab from the `Prefabs/` folder into your scene. Ensure all driveable surfaces have a collider attached to it.

If you want to setup a car from scratch, do the following:

1. Create an empty `Game Object` in your scene. 
2. Click `Add Component` and add the `Car` script.
3. With the new `Game Object` selected, adjust the `Car` script values to your desired vehicle specifications in Unity's Inspector.

**Note:** The `Car` script does not have its own camera or camera controller by default. You will need to add your own.
