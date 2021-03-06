﻿using ED_QuantumShield;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ED_NanoShield
{
    [StaticConstructorOnStartup]
    class CompQuantumShield : ThingComp
    {
        public int ChargeLevelCurrent = 200;

        public int ChargeLevelMax = 200;


        
        private static Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            using (IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    Gizmo c = enumerator.Current;
                    yield return c;
                    ;
                }
            }

            Gizmo_QuantumShieldStatus opt1 = new Gizmo_QuantumShieldStatus(this);
            yield return opt1;
            
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (absorbed)
            {
                return;
            }
            
            //Shield Depleted
            if (this.ChargeLevelCurrent == 0)
            {
                return;
            }

            this.ChargeLevelCurrent -= dinfo.Amount;
            if (this.ChargeLevelCurrent < 0)
            {
                this.ChargeLevelCurrent = 0;
            }


            absorbed = true;
            

        }

        public override void CompTick()
        {
            //Log.Message("CompTick");
            base.CompTick();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            float num1 = Mathf.Lerp(1.2f, 1.55f, 100f);
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.Blueprint);
            int num2 = 7;
            if (num2 < 8)
            {
                float num3 = (float)((double)(8 - num2) / 8.0 * 0.0500000007450581);
                num1 -= num3;
            }
            float angle = (float)Rand.Range(0, 360);
            Vector3 s = new Vector3(num1, 1f, num1);
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, CompQuantumShield.BubbleMat, 0);
        }

        public CompProperties_QuantumShield Props
        {
            get
            {
                return (CompProperties_QuantumShield)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            
        }
    }

    class CompProperties_QuantumShield : CompProperties
    {
        public CompProperties_QuantumShield()
        {
            this.compClass = typeof(CompQuantumShield);
        }
        
    }
}
