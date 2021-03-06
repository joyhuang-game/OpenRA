#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits
{
	class WithGunboatBodyInfo : WithSpriteBodyInfo, Requires<IBodyOrientationInfo>, Requires<IFacingInfo>, Requires<TurretedInfo>
	{
		[Desc("Turreted 'Turret' key to display")]
		public readonly string Turret = "primary";

		[SequenceReference] public readonly string LeftSequence = "left";
		[SequenceReference] public readonly string RightSequence = "right";
		[SequenceReference] public readonly string WakeLeftSequence = "wake-left";
		[SequenceReference] public readonly string WakeRightSequence = "wake-right";

		public override object Create(ActorInitializer init) { return new WithGunboatBody(init, this); }

		public override int QuantizedBodyFacings(ActorInfo ai, SequenceProvider sequenceProvider, string race)
		{
			return 2;
		}
	}

	class WithGunboatBody : WithSpriteBody, ITick
	{
		readonly WithGunboatBodyInfo info;
		readonly Animation wake;
		readonly RenderSprites rs;
		readonly IFacing facing;
		readonly Turreted turret;

		static Func<int> MakeTurretFacingFunc(Actor self)
		{
			// Turret artwork is baked into the sprite, so only the first turret makes sense.
			var turreted = self.TraitsImplementing<Turreted>().FirstOrDefault();
			return () => turreted.TurretFacing;
		}

		public WithGunboatBody(ActorInitializer init, WithGunboatBodyInfo info)
			: base(init, info, MakeTurretFacingFunc(init.Self))
		{
			this.info = info;
			rs = init.Self.Trait<RenderSprites>();
			facing = init.Self.Trait<IFacing>();
			var name = rs.GetImage(init.Self);
			turret = init.Self.TraitsImplementing<Turreted>()
				.First(t => t.Name == info.Turret);
			turret.QuantizedFacings = DefaultAnimation.CurrentSequence.Facings;

			wake = new Animation(init.World, name);
			wake.PlayRepeating(info.WakeLeftSequence);
			rs.Add(new AnimationWithOffset(wake, null, null, -87));
		}

		public void Tick(Actor self)
		{
			if (facing.Facing <= 128)
			{
				var left = NormalizeSequence(self, info.LeftSequence);
				if (DefaultAnimation.CurrentSequence.Name != left)
					DefaultAnimation.ReplaceAnim(left);

				if (wake.CurrentSequence.Name != info.WakeLeftSequence)
					wake.ReplaceAnim(info.WakeLeftSequence);
			}
			else
			{
				var right = NormalizeSequence(self, info.RightSequence);
				if (DefaultAnimation.CurrentSequence.Name != right)
					DefaultAnimation.ReplaceAnim(right);

				if (wake.CurrentSequence.Name != info.WakeRightSequence)
					wake.ReplaceAnim(info.WakeRightSequence);
			}
		}
	}
}
