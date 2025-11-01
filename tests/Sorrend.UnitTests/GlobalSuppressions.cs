[assembly: SuppressMessage(
    "Major Code Smell",
    "S4144:Methods should not have identical implementations",
    Justification = "Different test cases may have the same implementation",
    Scope = "namespaceanddescendants",
    Target = "~N:Sorrend.UnitTests.Scenarios")]
