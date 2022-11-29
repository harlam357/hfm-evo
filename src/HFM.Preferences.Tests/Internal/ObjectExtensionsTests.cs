using System.Diagnostics;

using HFM.Preferences.Data;

namespace HFM.Preferences.Internal;

[TestFixture]
public class ObjectExtensionsTests
{
    [Test]
    public void CopiesValueOrObject()
    {
        short a = 1;
        int b = 2;
        long c = 3;
        float d = 4.0f;
        double e = 5.0;
        string f = "foo";
        var g = new ClientRetrievalTask { Enabled = false };
        var h = new List<string>(new[] { "foo" });
        var i = new Dictionary<string, object> { { "foo", new object() } };

        Assert.Multiple(() =>
        {
            Assert.That(a.Copy(), Is.EqualTo(1));
            Assert.That(b.Copy(), Is.EqualTo(2));
            Assert.That(c.Copy(), Is.EqualTo(3));
            Assert.That(d.Copy(), Is.EqualTo(4.0f));
            Assert.That(e.Copy(), Is.EqualTo(5.0));
            Assert.That(f.Copy(), Is.EqualTo("foo"));
            Assert.That(g.Copy(), Is.Not.SameAs(g));
            Assert.That(h.Copy(), Is.Not.SameAs(h));
            Assert.That(i.Copy(), Is.Not.SameAs(i));
        });
    }

    [Test]
    public void CopiesValueOrObjectFromObject()
    {
        object objA = (short)1;
        object objB = 2;
        object objC = (long)3;
        object objD = 4.0f;
        object objE = 5.0;
        object objF = "foo";
        object objG = new ClientRetrievalTask { Enabled = false };
        object objH = new List<string>(new[] { "foo" });
        object objI = new Dictionary<string, object> { { "foo", new object() } };

        Assert.Multiple(() =>
        {
            Assert.That(objA.Copy(), Is.EqualTo(1));
            Assert.That(objB.Copy(), Is.EqualTo(2));
            Assert.That(objC.Copy(), Is.EqualTo(3));
            Assert.That(objD.Copy(), Is.EqualTo(4.0f));
            Assert.That(objE.Copy(), Is.EqualTo(5.0));
            Assert.That(objF.Copy(), Is.EqualTo("foo"));
            Assert.That(objG.Copy(), Is.Not.SameAs(objG));
            Assert.That(objH.Copy(), Is.Not.SameAs(objH));
            Assert.That(objI.Copy(), Is.Not.SameAs(objI));
        });
    }

    [Test]
    public void CopyBenchmark()
    {
        object? objNum = null;

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = objNum.Copy(typeof(int));
        }
        sw.Stop();
        Console.WriteLine("Copy Null ValueType: {0}ms", sw.ElapsedMilliseconds);

        int num = 1;

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = num.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy ValueType: {0}ms", sw.ElapsedMilliseconds);

        string? str = null;

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = str.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy Null String: {0}ms", sw.ElapsedMilliseconds);

        str = "foo";

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = str.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy String: {0}ms", sw.ElapsedMilliseconds);

        ClientRetrievalTask? task = null;

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = task.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy Null Class: {0}ms", sw.ElapsedMilliseconds);

        task = new ClientRetrievalTask();

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = task.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy Class: {0}ms", sw.ElapsedMilliseconds);

        List<string>? list = null;

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = list.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy Null List: {0}ms", sw.ElapsedMilliseconds);

        list = new List<string>(new[] { "a", "b", "c" });

        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _ = list.Copy();
        }
        sw.Stop();
        Console.WriteLine("Copy List: {0}ms", sw.ElapsedMilliseconds);
    }
}
