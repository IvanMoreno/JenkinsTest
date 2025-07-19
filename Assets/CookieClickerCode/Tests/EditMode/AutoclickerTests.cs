using System;
using CookieClickerCode.Runtime.Domain;
using CookieClickerCode.Runtime.Presenter;
using NUnit.Framework;

namespace CookieClickerCode.Tests.EditMode
{
    public class AutoclickerTests
    {
        [Test]
        public void MustNotAutoclick() {
            var cookieClicker = CookieClicker.CreateEmpty();
            var sut = new Autoclicker(cookieClicker);        
            cookieClicker.ClicksPerSecond = 2;
            
            var base_date = new DateTime();
            sut.AccumulateTime(base_date);
            Assert.IsFalse(sut.MustAutoclick(base_date + TimeSpan.FromSeconds(0.3f)));
        }

        [Test]
        public void MustAutoclick()
        {
            var cookieClicker = CookieClicker.CreateEmpty();
            var sut = new Autoclicker(cookieClicker);        
            cookieClicker.ClicksPerSecond = 2;
            
            var base_date = new DateTime();
            sut.AccumulateTime(base_date);
            Assert.IsTrue(sut.MustAutoclick(base_date + TimeSpan.FromSeconds(0.8)));
            Assert.IsTrue(sut.MustAutoclick(base_date + TimeSpan.FromSeconds(0.5)));

        }
    }
}