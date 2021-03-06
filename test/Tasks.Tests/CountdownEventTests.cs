﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class CountdownEventTests
    {
        [TestMethod]
        public void CountdownEvent_RunCountdownEventTest0_StateTrans()
        {
            RunCountdownEventTest0_StateTrans(0, 0, false);
            RunCountdownEventTest0_StateTrans(1, 0, false);
            RunCountdownEventTest0_StateTrans(128, 0, false);
            RunCountdownEventTest0_StateTrans(1024 * 1024, 0, false);
            RunCountdownEventTest0_StateTrans(1, 1024, false);
            RunCountdownEventTest0_StateTrans(128, 1024, false);
            RunCountdownEventTest0_StateTrans(1024 * 1024, 1024, false);
            RunCountdownEventTest0_StateTrans(1, 0, true);
            RunCountdownEventTest0_StateTrans(128, 0, true);
            RunCountdownEventTest0_StateTrans(1024 * 1024, 0, true);
            RunCountdownEventTest0_StateTrans(1, 1024, true);
            RunCountdownEventTest0_StateTrans(128, 1024, true);
            RunCountdownEventTest0_StateTrans(1024 * 1024, 1024, true);
        }

        [TestMethod]
        public void CountdownEvent_RunCountdownEventTest1_SimpleTimeout()
        {
            RunCountdownEventTest1_SimpleTimeout(0);
            RunCountdownEventTest1_SimpleTimeout(100);
        }

        // Validates init, set, reset state transitions.
        private static void RunCountdownEventTest0_StateTrans(int initCount, int increms, bool takeAllAtOnce)
        {
            CountdownEvent ev = new CountdownEvent(initCount);

            Assert.AreEqual(initCount, ev.InitialCount);

            // Increment (optionally).
            for (int i = 1; i < increms + 1; i++)
            {
                ev.AddCount();
                Assert.AreEqual(initCount + i, ev.CurrentCount);
            }

            // Decrement until it hits 0.
            if (takeAllAtOnce)
            {
                ev.Signal(initCount + increms);
            }
            else
            {
                for (int i = 0; i < initCount + increms; i++)
                {
                    Assert.IsFalse(ev.IsSet, string.Format("  > error: latch is set after {0} signals", i));
                    ev.Signal();
                }
            }

            Assert.IsTrue(ev.IsSet);
            Assert.AreEqual(0, ev.CurrentCount);

            // Now reset the event and check its count.
            ev.Reset();
            Assert.AreEqual(ev.InitialCount, ev.CurrentCount);
        }

        // Tries some simple timeout cases.
        private static void RunCountdownEventTest1_SimpleTimeout(int ms)
        {
            // Wait on the event.
            CountdownEvent ev = new CountdownEvent(999);
            Assert.IsFalse(ev.Wait(ms));
            Assert.IsFalse(ev.IsSet);
            Assert.IsFalse(ev.WaitHandle.WaitOne(ms));
        }

        [TestMethod]
        public void CountdownEvent_RunCountdownEventTest2_Exceptions()
        {
            CountdownEvent cde = null;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde = new CountdownEvent(-1));
            // Failure Case: Constructor didn't throw AORE when -1 passed

            cde = new CountdownEvent(1);
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Signal(0));
            // Failure Case: Signal didn't throw AORE when 0 passed

            cde = new CountdownEvent(0);
            AssertExtensions.Throws<InvalidOperationException>(() => cde.Signal());
            // Failure Case: Signal didn't throw IOE when the count is zero

            cde = new CountdownEvent(1);
            AssertExtensions.Throws<InvalidOperationException>(() => cde.Signal(2));
            // Failure Case: Signal didn't throw IOE when the signal count > current count

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.AddCount(0));
            // Failure Case: AddCount didn't throw AORE when 0 passed

            cde = new CountdownEvent(0);
            AssertExtensions.Throws<InvalidOperationException>(() => cde.AddCount(1));
            // Failure Case: AddCount didn't throw IOE when the count is zero

            cde = new CountdownEvent(int.MaxValue - 10);
            AssertExtensions.Throws<InvalidOperationException>(() => cde.AddCount(20));
            // Failure Case: AddCount didn't throw IOE when the count > int.Max

            cde = new CountdownEvent(2);
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Reset(-1));
            // Failure Case: Reset didn't throw AORE when the count is zero

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Wait(-2));
            // Failure Case: Wait(int) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Wait(TimeSpan.FromDays(-1)));
            // Failure Case:  FAILED.  Wait(TimeSpan) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Wait(TimeSpan.MaxValue));
            // Failure Case: Wait(TimeSpan, CancellationToken) didn't throw AORE when the totalmilliseconds > int.max

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Wait(TimeSpan.FromDays(-1), new CancellationToken()));
            // Failure Case: Wait(TimeSpan) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => cde.Wait(TimeSpan.MaxValue, new CancellationToken()));
            // Failure Case: Wait(TimeSpan, CancellationToken) didn't throw AORE when the totalmilliseconds > int.max

            cde.Dispose();

            AssertExtensions.Throws<ObjectDisposedException>(() => cde.Wait());
            // Failure Case: Wait() didn't throw ODE after Dispose
        }
    }
}