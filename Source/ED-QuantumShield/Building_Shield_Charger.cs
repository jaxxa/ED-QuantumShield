﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ED_QuantumShield
{
    [StaticConstructorOnStartup]
    public class Building_QuantumShield_Charger : Building
    {
        #region Variables

        private int ReservePowerCurrent = 0;
        private int ReservePowerMax = 400;

        public float MAX_DISTANCE = 5.0f;
        public bool flag_charge = false;
        CompPowerTrader power;

        private static Texture2D UI_UPGRADE;
        private static Texture2D UI_CHARGE_OFF;
        private static Texture2D UI_CHARGE_ON;

        #endregion

        //Constructor
        static Building_QuantumShield_Charger()
        {
            //Log.Message("Getting graphics");
            UI_UPGRADE = ContentFinder<Texture2D>.Get("UI/Upgrade", true);
            UI_CHARGE_OFF = ContentFinder<Texture2D>.Get("UI/ChargeOFF", true);
            UI_CHARGE_ON = ContentFinder<Texture2D>.Get("UI/ChargeON", true);
        }

        #region Overrides

        //Dummy override
        public override void PostMake()
        {
            base.PostMake();
        }
        public override void Draw()
        {
            base.Draw();
        }

        //On spawn, get the power component reference
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.power = base.GetComp<CompPowerTrader>();
        }

        public override void Tick()
        {
            //Log.Message("Tick");
            base.Tick();


            int currentTick = Find.TickManager.TicksGame;
            //Only every 10 ticks
            if (currentTick % 10 != 0)
            {
                return;
            }
            if (this.power.PowerOn == true)
            {
                if (this.flag_charge)
                {
                    this.rechargePawns(10);
                }

                if (this.ReservePowerCurrent < this.ReservePowerMax)
                {
                    this.ReservePowerCurrent += 1;
                }
            }
            else
            {

                if (this.flag_charge)
                {
                    if (this.ReservePowerCurrent > 0)
                    {
                        this.ReservePowerCurrent -= 1;
                        this.ReservePowerCurrent += this.rechargePawns(1);
                    }
                }
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(base.Position, this.MAX_DISTANCE);
        }
        
        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref flag_charge, "flag_charge");
            Scribe_Values.Look(ref ReservePowerCurrent, "ReservePowerCurrent");
            Scribe_Values.Look(ref ReservePowerMax, "ReservePowerMax");

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.upgradePawns();
                act.icon = UI_UPGRADE;
                act.defaultLabel = "Upgrade Pawn";
                act.defaultDesc = "Upgrade Pawn";
                //act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

            if (flag_charge)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.SwitchCharge();
                act.icon = UI_CHARGE_ON;
                act.defaultLabel = "Charge Shields";
                act.defaultDesc = "On";
                // act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
            else
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.SwitchCharge();
                act.icon = UI_CHARGE_OFF;
                act.defaultLabel = "Charge Shields";
                act.defaultDesc = "Off";
                //act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        #endregion

        private void SwitchCharge()
        {
            flag_charge = !flag_charge;
        }

        private IEnumerable<CompQuantumShield> ShieldCompsInRangeAndOfFaction()
        {
            IEnumerable<Pawn> _Pawns = this.Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where<Pawn>(t => t.Position.InHorDistOf(this.Position, this.MAX_DISTANCE));
            IEnumerable<CompQuantumShield> _Comps = _Pawns.Select(p => p.TryGetComp<CompQuantumShield>());
            return _Comps;
        }

        private void upgradePawns()
        {
            bool _AnyUpgraded = false;
            
            foreach (CompQuantumShield _ShieldComp in this.ShieldCompsInRangeAndOfFaction())
            {
                Log.Message("Adding");
                if (!_ShieldComp.QuantumShieldActive)
                {
                    _ShieldComp.QuantumShieldActive = true;
                    _AnyUpgraded = true;
                }
            }

            if (!_AnyUpgraded)
            {
                Log.Message("No Paws found to add Quantum Shields to.");
            }

            return;
        }

        public int rechargePawns(int chargeToRequest)
        {
            int _RemainingCharge = GameComponent_QuantumShield.RequestCharge(chargeToRequest);

            foreach (CompQuantumShield _ShieldComp in this.ShieldCompsInRangeAndOfFaction())
            {
                if (_ShieldComp.QuantumShieldActive)
                {
                    _RemainingCharge -= _ShieldComp.RechargeShield(_RemainingCharge);
                }
            }

            GameComponent_QuantumShield.ReturnCharge(_RemainingCharge);
            return _RemainingCharge;
        }

        public override string GetInspectString()
        {

            return "Reserve = " + this.ReservePowerCurrent + " / " + this.ReservePowerMax + " - "  + GameComponent_QuantumShield.GetInspectStringStatus() + Environment.NewLine + base.GetInspectString();
        }

    }
}