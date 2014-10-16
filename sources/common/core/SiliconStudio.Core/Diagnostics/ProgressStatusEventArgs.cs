// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// An event indicating the progress of an operation.
    /// </summary>
    public class ProgressStatusEventArgs : EventArgs
    {
        private readonly string message;
        private readonly int currentStep;
        private readonly int stepCount;
        private readonly bool hasKnownSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatusEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ProgressStatusEventArgs(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            this.message = message;
            stepCount = 1;
            hasKnownSteps = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatusEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="currentStep">The current step.</param>
        /// <param name="stepCount">The step count.</param>
        public ProgressStatusEventArgs(string message, int currentStep, int stepCount)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (currentStep < 0) throw new ArgumentOutOfRangeException("currentStep", "Expecting value >= 0");
            if (stepCount < 1) throw new ArgumentOutOfRangeException("stepCount", "Expecting value >= 1");
            this.message = message;
            this.currentStep = currentStep;
            this.stepCount = stepCount;
            this.hasKnownSteps = true;
        }

        /// <summary>
        /// Gets or sets the message associated with the progress.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return message;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has known steps (<see cref="CurrentStep"/> and 
        /// <see cref="StepCount"/> are valid).
        /// </summary>
        /// <value><c>true</c> if this instance has known steps; otherwise, <c>false</c>.</value>
        public bool HasKnownSteps
        {
            get
            {
                return hasKnownSteps;
            }
        }

        /// <summary>
        /// Gets or sets the current index of the indicative step. See <see cref="StepCount"/> remarks.
        /// </summary>
        /// <value>The index of the step.</value>
        public int CurrentStep
        {
            get
            {
                return currentStep;
            }
        }

        /// <summary>
        /// Gets or sets the step count used to indicate the number expected steps returned by this logger result. See remarks.
        /// </summary>
        /// <value>The step count, greater than 1. Default is 1</value>
        /// <exception cref="System.ArgumentOutOfRangeException">Expecting value >= 1;value</exception>
        /// <remarks>
        /// This property providea an estimation of the duration of an operation in terms of "step counts". 
        /// The <see cref="CurrentStep"/> property returns the current step and gives indication about how much is still
        /// being processed.
        /// </remarks>
        public int StepCount
        {
            get
            {
                return stepCount;
            }
        }
    }
}