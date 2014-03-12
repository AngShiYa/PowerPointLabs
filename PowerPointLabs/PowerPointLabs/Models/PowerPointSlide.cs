﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Shape = Microsoft.Office.Interop.PowerPoint.Shape;
using ShapeRange = Microsoft.Office.Interop.PowerPoint.ShapeRange;
using Shapes = Microsoft.Office.Interop.PowerPoint.Shapes;

namespace PowerPointLabs.Models
{
    class PowerPointSlide
    {
        protected readonly Slide _slide;

        protected PowerPointSlide(Slide slide)
        {
            _slide = slide;
        }

        public static PowerPointSlide FromSlideFactory(Slide slide)
        {
            if (slide == null)
            {
                return null;
            }

            if (slide.Name.Contains("PPTLabsSpotlight"))
                return PowerPointSpotlightSlide.FromSlideFactory(slide);
            else if (slide.Name.Contains("PPTLabsAck"))
                return PowerPointAckSlide.FromSlideFactory(slide);
            else
                return new PowerPointSlide(slide);
        }

        public String NotesPageText
        {
            get
            {
                if (_slide == null || _slide.HasNotesPage == MsoTriState.msoFalse)
                {
                    return String.Empty;
                }

                IEnumerable<Shape> notesPagePlaceholders = _slide.NotesPage.Shapes.Placeholders.Cast<Shape>();
                Shape notesPageBody = notesPagePlaceholders.FirstOrDefault(shape => shape.PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody);

                String notesText = notesPageBody != null ? notesPageBody.TextFrame.TextRange.Text : String.Empty;
                return notesText;
            }
        }

        public Shapes Shapes
        {
            get { return _slide.Shapes; }
        }

        public int Index
        {
            get { return _slide.SlideIndex; }
        }

        public SlideShowTransition Transition
        {
            get { return _slide.SlideShowTransition; }
        }

        public void DeleteShapesWithPrefix(string prefix)
        {
            List<Shape> shapes = _slide.Shapes.Cast<Shape>().ToList();

            var matchingShapes = shapes.Where(current => current.Name.StartsWith(prefix));
            foreach (Shape s in matchingShapes)
            {
                s.Delete();
            }
        }

        public void SetAudioAsAutoplay(Shape shape)
        {
            var mainSequence = _slide.TimeLine.MainSequence;

            Effect firstClickEvent = mainSequence.FindFirstAnimationForClick(1);
            bool hasNoClicksOnSlide = firstClickEvent == null;

            if (hasNoClicksOnSlide)
            {
                AddShapeAsLastAutoplaying(shape, MsoAnimEffect.msoAnimEffectMediaPlay);
            }
            else
            {
                InsertAnimationBeforeExisting(shape, firstClickEvent, MsoAnimEffect.msoAnimEffectMediaPlay);
            }
        }

        private Effect InsertAnimationBeforeExisting(Shape shape, Effect existing, MsoAnimEffect effect)
        {
            var sequence = _slide.TimeLine.MainSequence;

            Effect newAnimation = sequence.AddEffect(shape, effect, MsoAnimateByLevel.msoAnimateLevelNone,
                MsoAnimTriggerType.msoAnimTriggerWithPrevious);
            newAnimation.MoveBefore(existing);

            return newAnimation;
        }

        public Effect SetShapeAsClickTriggered(Shape shape, int clickNumber, MsoAnimEffect effect)
        {
            Effect addedEffect;

            Sequence mainSequence = _slide.TimeLine.MainSequence;
            Effect nextClickEffect = mainSequence.FindFirstAnimationForClick(clickNumber + 1);
            Effect previousClickEffect = mainSequence.FindFirstAnimationForClick(clickNumber);

            bool hasClicksAfter = nextClickEffect != null;
            bool hasClickBefore = previousClickEffect != null;

            if (hasClicksAfter)
            {
                addedEffect = InsertAnimationBeforeExisting(shape, nextClickEffect, effect);
            }
            else if (hasClickBefore)
            {
                addedEffect = AddShapeAsLastAutoplaying(shape, effect);
            }
            else
            {
                addedEffect = mainSequence.AddEffect(shape, effect);
            }

            return addedEffect;
        }

        private Effect AddShapeAsLastAutoplaying(Shape shape, MsoAnimEffect effect)
        {
            Effect addedEffect = _slide.TimeLine.MainSequence.AddEffect(shape, effect,
                MsoAnimateByLevel.msoAnimateLevelNone, MsoAnimTriggerType.msoAnimTriggerWithPrevious);
            return addedEffect;
        }

        private Effect InsertAnimationAtIndex(Shape shape, int index, MsoAnimEffect animationEffect,
            MsoAnimTriggerType triggerType)
        {
            var animationSequence = _slide.TimeLine.MainSequence;
            Effect effect = animationSequence.AddEffect(shape, animationEffect, MsoAnimateByLevel.msoAnimateLevelNone,
                triggerType);
            effect.MoveTo(index);
            return effect;
        }

        public void ShowShapeAfterClick(Shape shape, int clickNumber)
        {
            SetShapeAsClickTriggered(shape, clickNumber, MsoAnimEffect.msoAnimEffectAppear);
        }

        public void HideShapeAfterClick(Shape shape, int clickNumber)
        {
            Effect addedEffect = SetShapeAsClickTriggered(shape, clickNumber, MsoAnimEffect.msoAnimEffectAppear);
            addedEffect.Exit = MsoTriState.msoTrue;
        }

        public void HideShapeAsLastClickIfNeeded(Shape shape)
        {
            if (IsNextSlideTransitionBlacklisted())
            {
                var animationSequence = _slide.TimeLine.MainSequence;
                var effect = animationSequence.AddEffect(shape, MsoAnimEffect.msoAnimEffectFade);
                effect.Exit = MsoTriState.msoTrue;
            }
        }

        private bool IsNextSlideTransitionBlacklisted()
        {
            bool isLastSlide = _slide.SlideIndex == PowerPointPresentation.SlideCount;
            if (isLastSlide)
            {
                return false;
            }

            // Indexes are from 1, while the slide collection starts from 0.
            PowerPointSlide nextSlide = PowerPointPresentation.Slides.ElementAt(Index);
            switch (nextSlide.Transition.EntryEffect)
            {
                case PpEntryEffect.ppEffectCoverUp:
                case PpEntryEffect.ppEffectCoverLeftUp:
                case PpEntryEffect.ppEffectCoverRightUp:
                case PpEntryEffect.ppEffectFlyFromBottom:
                case PpEntryEffect.ppEffectPushUp:
                case PpEntryEffect.ppEffectPushDown:
                case PpEntryEffect.ppEffectSwitchUp:
                case PpEntryEffect.ppEffectFlipUp:
                case PpEntryEffect.ppEffectCubeUp:
                case PpEntryEffect.ppEffectRotateUp:
                case PpEntryEffect.ppEffectBoxUp:
                case PpEntryEffect.ppEffectOrbitUp:
                case PpEntryEffect.ppEffectPanUp:
                    return true;
                default:
                    return false;
            }
        }

        public void TransferAnimation(Shape source, Shape destination)
        {
            Sequence sequence = _slide.TimeLine.MainSequence;
            var enumerableSequence = sequence.Cast<Effect>().ToList();

            Effect entryDetails = enumerableSequence.FirstOrDefault(effect => effect.Shape.Equals(source));
            if (entryDetails != null)
            {
                InsertAnimationAtIndex(destination, entryDetails.Index, entryDetails.EffectType, entryDetails.Timing.TriggerType);
            }

            Effect exitDetails = enumerableSequence.Last(effect => effect.Shape.Equals(source));
            if (exitDetails != null && !exitDetails.Equals(entryDetails))
            {
                InsertAnimationAtIndex(destination, exitDetails.Index, exitDetails.EffectType,
                    exitDetails.Timing.TriggerType);
            }
        }

        public void RemoveAnimationsForShape(Shape shape)
        {
            IEnumerable<Effect> mainEffects = _slide.TimeLine.MainSequence.Cast<Effect>();
            DeleteEffectsForShape(shape, mainEffects);

            foreach (Sequence sequence in _slide.TimeLine.InteractiveSequences)
            {
                DeleteEffectsForShape(shape, sequence.Cast<Effect>());
            }
        }

        private static void DeleteEffectsForShape(Shape shape, IEnumerable<Effect> mainEffects)
        {
            foreach (Effect e in mainEffects.Where(e => e.Shape.Equals(shape)))
            {
                e.Delete();
            }
        }

        public PowerPointSlide CreateSpotlightSlide()
        {
            Slide duplicatedSlide = _slide.Duplicate()[1];
            return PowerPointSpotlightSlide.FromSlideFactory(duplicatedSlide);
        }

        protected void DeleteSlideNotes()
        {
            if (_slide.HasNotesPage == MsoTriState.msoTrue)
            {
                foreach (Shape sh in _slide.NotesPage.Shapes)
                {
                    if (sh.TextFrame.HasText == MsoTriState.msoTrue)
                        sh.TextEffect.Text = "";
                }
            }
        }

        protected void DeleteSlideMedia()
        {
            foreach (Shape sh in _slide.Shapes)
            {
                if (sh.Type == MsoShapeType.msoMedia)
                    sh.Delete();
            }
        }

        public bool isSpotlightSlide()
        {
            return _slide.Name.Contains("PPTLabsSpotlight");
        }

        public bool isAckSlide()
        {
            return _slide.Name.Contains("PPTLabsAck");
        }

        public PowerPointSlide CreateAckSlide()
        {
            Slide ackSlide = Globals.ThisAddIn.Application.ActivePresentation.Slides.Add(Globals.ThisAddIn.Application.ActivePresentation.Slides.Count + 1, PpSlideLayout.ppLayoutBlank);
            return PowerPointAckSlide.FromSlideFactory(ackSlide);
        }
    }
}
