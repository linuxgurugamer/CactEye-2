﻿using System;
using System.Linq;
using UnityEngine;
using static CactEye2.InitialSetup;
using PartWrapper;
using Random = System.Random;

namespace CactEye2
{

    public class CactEyeOptics : PartModule
    {
        [KSPField(isPersistant = false)]
        public bool DebugMode = true;

        [KSPField(isPersistant = false)]
        public bool IsSmallOptics = false;

        [KSPField(isPersistant = false)]
        public string CameraTransformName = "CactEyeCam";

        [KSPField(isPersistant = false)]
        public bool IsFunctional = false;

        [KSPField(isPersistant = false)]
        public float scienceMultiplier;

        [KSPField(isPersistant = true)]
        public bool IsDamaged = false;

        [KSPField(isPersistant = true)]
        public bool SmallApertureOpen = false;

        [KSPField(isPersistant = true)]
        public bool isRBInstalled;

        [KSPField(isPersistant = true)]
        public bool isRBEnabled;

        [KSPField(isPersistant = false)]
        public string CameraPartOwner = "CactEye";

        [KSPField]
        public bool ProcessorNeeded = true;

        [KSPField]
        public Vector3 cameraPosition = Vector3.zero;
        [KSPField]
        public Vector3 cameraForward = Vector3.forward;
        [KSPField]
        public Vector3 cameraUp = Vector3.zero;

        public RBWrapper ResearchBodies;

        internal CactEyeProcessor ActiveProcessor;


#if false
        private ModuleAnimateGeneric opticsAnimate;
#endif

        //Control Variable that disables functionality if there is an error.
        //private bool Error = false;

        private TelescopeMenu TelescopeControlMenu;
        private PartWrapper.PartWrapper partWrapper;


        /*
         * Function name: OnStart
         * Purpose: This overrides the OnStart functionality. This function will be called once
         * at the start of the game directly after a scene load. In this case, the function will
         * instatiate the GUI.
         */

        internal Transform localCameratransform;
        public override void OnStart(StartState state)
        {
            partWrapper = new PartWrapper.PartWrapper();
            partWrapper.InitPartWrapper(this.part, CameraPartOwner);

#if false
            if (!IsSmallOptics)
            {
                opticsAnimate = GetComponent<ModuleAnimateGeneric>();
            }
            else if (IsSmallOptics && !SmallApertureOpen)
            {
                Events["OpenSmallAperture"].active = true;
            }
#else
            if (IsSmallOptics && !SmallApertureOpen)
            {
                Events["OpenSmallAperture"].active = true;
            }

#endif
            //Attempt to instantiate the GUI
             localCameratransform = part.FindModelTransform(CameraTransformName);
            if (localCameratransform == null)
            {
                Log.Info("CameraTransformName not found: " + CameraTransformName);
                return;
            }
            if (cameraPosition != Vector3.zero || cameraUp != Vector3.zero || cameraForward != Vector3.forward)
            {
                Log.Info("Setting camera position");
                localCameratransform.localPosition = cameraPosition;
                localCameratransform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
            }

            try
            {
                TelescopeControlMenu = new TelescopeMenu(localCameratransform);

                TelescopeControlMenu.scienceMultiplier = this.scienceMultiplier;
                TelescopeControlMenu.SetSmallOptics(IsSmallOptics);
                TelescopeControlMenu.SetScopeOpen(IsFunctional);
#if false
                if (!IsSmallOptics)
                {
                    TelescopeControlMenu.SetAperature(opticsAnimate);
                }
#else
                TelescopeControlMenu.SetSDWrapper(partWrapper);
#endif
            }
            catch (Exception E)
            {
                //Error = true;
                Log.Error("Exception 1:  Was not able to create the Telescope Control Menu object. You should try re-installing CactEye2 and ensure that old versions of CactEye are deleted.");
                Log.Error(E.ToString());
                Log.Error(localCameratransform.ToString());
            }

            if (IsSmallOptics && SmallApertureOpen && !IsDamaged)
            {
                IsFunctional = true;
            }

            if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
            {
                Log.Info("Debug: SmallApertureOpen is " + SmallApertureOpen.ToString());
                Log.Info("Debug: IsSmallOptics is " + IsSmallOptics.ToString());
                Log.Info("Debug: IsFunctional is " + IsFunctional.ToString());
                Log.Info("Debug: IsDamaged is " + IsDamaged.ToString());
            }
        }

#if false
        /* ************************************************************************************************
         * Function Name: IsModInstalled
         * Input: Mod Name
         * Output: True if mod is found
         * Purpose: This function will be called at start to determine if a particular mod is installed
         * ************************************************************************************************/
        public static bool IsModInstalled(string assemblyName)
        {
            return AssemblyLoader.loadedAssemblies.Any(a => a.name == assemblyName);

        }
#endif

        /* ************************************************************************************************
         * Function Name: FixedUpdate
         * Input: None
         * Output: None
         * Purpose: This function will run once every physics frame. It is used for some event handling, and for
         * updating information such as the scope orientation and position. It will also check for the 
         * condition of when the telescope is pointed at the sun, and will damage the scope if it
         * detects excessive sun exposure.
         * ************************************************************************************************/
        //public override void OnUpdate()
        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;
            //Enable Repair Scope context menu option if the scope is damaged.
            if (IsDamaged)
            {
                IsFunctional = false;
                Events["FixScope"].active = true;
            }

            //If the scope isn't damage, then toggle scope functionality based on the aperture.
            else
            {
#if false
                if (opticsAnimate != null && !IsSmallOptics)
                {
                    if (opticsAnimate.animTime < 0.5 && IsFunctional)
                    {
                        IsFunctional = false;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                    if (opticsAnimate.animTime > 0.5 && !IsFunctional)
                    {
                        IsFunctional = true;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                }
#else
                if (!IsSmallOptics)
                {
                    if (partWrapper.animTime < 0.5 && IsFunctional)
                    {
                        IsFunctional = false;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                    if (partWrapper.animTime > 0.5 && !IsFunctional)
                    {
                        IsFunctional = true;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                }

#endif

                //If the aperture is opened and the Sun is not occulted, and a solar filter is not enabled
                if (IsFunctional && CactEyeAPI.CheckOccult(Planetarium.fetch.Sun) == "" &&(ActiveProcessor != null && ActiveProcessor.Type == "Occultation" && !((CactEyeOccultationProcessor)ActiveProcessor).solarFilter))
                {

                    //Check if we're pointing at the sun
                    Vector3d Heading = (Planetarium.fetch.Sun.position - FlightGlobals.ship_position).normalized;
                    if (Vector3d.Dot(localCameratransform.up, Heading) > 0.9 && HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().SunDamage)
                    {
                        ScreenMessages.PostScreenMessage("Telescope pointed directly at sun, optics damaged and processors fried!", 6, ScreenMessageStyle.UPPER_CENTER);
                        BreakScope();
                        DestroyProcessors();
                        //Destroy all the processors, should be fun :)
                    }
                    else if (Vector3d.Dot(localCameratransform.up, Heading) > 0.85)
                    {
                        ScreenMessages.PostScreenMessage("Telescope is getting close to the sun. Please make course adjustements before an equipment failure happens!", 6, ScreenMessageStyle.UPPER_CENTER);
                        CheckWarp();
                    }
                }
            }

            //Send updated position information to the telescope gui object.
            //if (TelescopeControlMenu != null)
            if (TelescopeControlMenu.IsGUIVisible)
            {
                TelescopeControlMenu.FixedUpdate(part, this);
            }
        }

  
        float timeHalted = 0;
        /* ************************************************************************************************
         * Function Name: CheckWarp
         * Input: None
         * Output: None
         * Purpose: This function will stop timewarp if telescope gets  too close to sun.  Only once every 30 seconds
         * ************************************************************************************************/
        private void CheckWarp()
        {
            if ( Time.realtimeSinceStartup - timeHalted > 30f)
            {
                TimeWarp.SetRate(0, true);
                timeHalted = Time.realtimeSinceStartup;
            }
        }


        /* ************************************************************************************************
         * Function Name: BreakScope
         * Input: None
         * Output: None
         * Purpose: This function will "damage" the telescope and will render the telescope inoperable.
         * ************************************************************************************************/
        public void BreakScope()
        {
            IsFunctional = false;
            IsDamaged = true;
        }

        /* ************************************************************************************************
         * Function Name: DestroyProcessors
         * Input: None
         * Output: None
         * Purpose: This function will destroy all onboard processors. 
         * ************************************************************************************************/
        public void DestroyProcessors()
        {
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                CactEyeProcessor cpu = p.GetComponent<CactEyeProcessor>();
                if (cpu != null)
                {
                    cpu.Die(); //Good-bye!
                }
            }
        }

        /* ************************************************************************************************
         * Function Name: OnMenuEnabled
         * Input: None
         * Output: Boolean value
         * Purpose: This function will check to see if the telescope control menu is open.
         * ************************************************************************************************/
        public bool IsMenuEnabled()
        {
            try
            {
                return TelescopeControlMenu.IsMenuEnabled();
            }
            catch
            {
                Log.Error("CactEye 2: Unknown Exception. If this is thrown before the vessel is unpacked, then it can be safely ignored.");
                return false;
            }
        }

        /* ************************************************************************************************
         * Function Name: GetFOV
         * Input: None
         * Output: The current field of view.
         * Purpose: This function will check for and return the current field of view of the telescope.
         * Used to allow other classes to check the current zoom of the scope.
         * ************************************************************************************************/
        public float GetFOV()
        {
            return TelescopeControlMenu.GetFOV();
        }

        /* ************************************************************************************************
         * Function Name: controlFromHere
         * Input: None
         * Output: None
         * Purpose: This is an event that is activated by a right click action on the scope. This function
         * will set the scope as the reference part for control of the craft.
         * ************************************************************************************************/
        [KSPEvent(guiActive = true, guiName = "Control from Here", active = true)]
        public void controlFromHere()
        {
            vessel.SetReferenceTransform(part);
        }

        /* ************************************************************************************************
         * Function Name: ToggleGUI
         * Input: None
         * Output: None
         * Purpose: This function will either open or close the telescope control menu only if the scope 
         * is not damaged and is operable. This will throw an exception if there are problems; the 
         * exception thrown by this function should never be thrown by a player's computer, unless there
         * is something horribly wrong with either the CactEye installation, the KSP installation, or 
         * the player's computer. This could be, in rare cases, thrown when the player's computer runs
         * out of memory.
         * ************************************************************************************************/
        [KSPEvent(guiActive = true, guiName = "Toggle CactEye GUI", active = true)]
        public void ToggleGUI()
        {
            if (!IsDamaged)
            {
                try
                {
                    TelescopeControlMenu.Toggle(this);
                }
                catch (Exception E)
                {
                    Log.Error("Exception 3: Was not able to bring up the Telescope Control Menu. The Telescope Control Menu returned a null reference.");
                    Log.Error(E.ToString());
                }
            }
            else
            {
                //Display error message that scope is damaged.
                ScreenMessages.PostScreenMessage("Telescope optics are damaged! Telescope needs to be repaired by EVA!", 6, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void OnGUI()
        {
            if (TelescopeControlMenu.IsGUIVisible && !FlightDriver.Pause)
                TelescopeControlMenu.DrawGUI();
        }

        /* ************************************************************************************************
         * Function Name: FixScope
         * Input: None
         * Output: None
         * Purpose: This function will render a damaged scope operable again. This is activated via EVA
         * through the right click menu.
         * ************************************************************************************************/
        [KSPEvent(active = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repair Optics", unfocusedRange = 5)]
        public void FixScope()
        {
            IsDamaged = false;
            IsFunctional = true;
            Events["FixScope"].active = false;
        }

        /* ************************************************************************************************
         * Function Name: OpenSmallAperture
         * Input: None
         * Output: None
         * Purpose: This function will open the aperture on a FungEye optics system. This may be deprecated
         * sometime in the future.
         * ************************************************************************************************/
        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Open Aperture (permanent!)", unfocusedRange = 2)]
        public void OpenSmallAperture()
        {
            SmallApertureOpen = true;
            IsFunctional = true;
            Events["OpenSmallAperture"].active = false;
        }
    }
}
