namespace Cirreum.ResultMonad.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

[TestClass]
public class OptionalSerializationTests {

	private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

	[TestMethod]
	public void Present_RoundTrips_Transparently() {
		var json = JsonSerializer.Serialize(Optional<int>.For(5));
		Assert.AreEqual("5", json, "A present optional serializes as its bare value.");

		var back = JsonSerializer.Deserialize<Optional<int>>(json);
		Assert.IsTrue(back.HasValue);
		Assert.AreEqual(5, back.Value);
	}

	[TestMethod]
	public void Empty_SerializesToNull_AndRoundTrips() {
		var json = JsonSerializer.Serialize(Optional<int>.Empty);
		Assert.AreEqual("null", json, "An empty optional serializes as null.");

		var back = JsonSerializer.Deserialize<Optional<int>>(json);
		Assert.IsTrue(back.IsEmpty);
	}

	[TestMethod]
	public void Present_Reference_RoundTrips() {
		var back = JsonSerializer.Deserialize<Optional<string>>(
			JsonSerializer.Serialize(Optional<string>.For("hi")));

		Assert.IsTrue(back.HasValue);
		Assert.AreEqual("hi", back.Value);
	}

	[TestMethod]
	public void Present_Complex_RoundTrips_UnderWebOptions() {
		var original = Optional<SampleDto>.For(new SampleDto("Ada", 36));

		var back = JsonSerializer.Deserialize<Optional<SampleDto>>(
			JsonSerializer.Serialize(original, Web), Web);

		Assert.IsTrue(back.HasValue);
		Assert.AreEqual(original.Value, back.Value);
	}

	[TestMethod]
	public void AsProperty_PresentAndEmpty_RoundTrip() {
		var holder = new Holder {
			Name = Optional<string>.For("Ada"),
			Nickname = Optional<string>.Empty
		};

		var back = JsonSerializer.Deserialize<Holder>(JsonSerializer.Serialize(holder, Web), Web)!;

		Assert.IsTrue(back.Name.HasValue);
		Assert.AreEqual("Ada", back.Name.Value);
		Assert.IsTrue(back.Nickname.IsEmpty);
	}

	public sealed record SampleDto(string Name, int Age);

	public sealed class Holder {
		public Optional<string> Name { get; set; }
		public Optional<string> Nickname { get; set; }
	}

}
