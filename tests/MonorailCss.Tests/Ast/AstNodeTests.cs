using System.Collections.Immutable;
using MonorailCss.Ast;
using Shouldly;

namespace MonorailCss.Tests.Ast;

public class AstNodeTests
{
    [Fact]
    public void Declaration_Should_Create_With_Property_And_Value()
    {
        var decl = new Declaration("color", "red");

        decl.Property.ShouldBe("color");
        decl.Value.ShouldBe("red");
        decl.Important.ShouldBe(false);
    }

    [Fact]
    public void Declaration_Should_Create_With_Important_Flag()
    {
        var decl = new Declaration("color", "red", true);

        decl.Property.ShouldBe("color");
        decl.Value.ShouldBe("red");
        decl.Important.ShouldBe(true);
    }

    [Fact]
    public void Declaration_Equality_Should_Work()
    {
        var decl1 = new Declaration("color", "red");
        var decl2 = new Declaration("color", "red");
        var decl3 = new Declaration("color", "blue");

        decl1.ShouldBe(decl2);
        decl1.ShouldNotBe(decl3);
    }

    [Fact]
    public void Declaration_With_Expression_Should_Create_New_Instance()
    {
        var decl1 = new Declaration("color", "red");
        var decl2 = decl1 with { Value = "blue" };

        decl1.Value.ShouldBe("red");
        decl2.Value.ShouldBe("blue");
        decl1.ShouldNotBe(decl2);
    }

    [Fact]
    public void StyleRule_Should_Create_With_Selector_And_Nodes()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "red"),
            new Declaration("background", "blue")
        );

        var rule = new StyleRule(".test", nodes);

        rule.Selector.ShouldBe(".test");
        rule.Nodes.Count.ShouldBe(2);
    }

    [Fact]
    public void StyleRule_Equality_Should_Work()
    {
        // ImmutableList uses reference equality by default,
        // so same list instance will be equal
        var nodes = ImmutableList.Create<AstNode>(new Declaration("color", "red"));

        var rule1 = new StyleRule(".test", nodes);
        var rule2 = new StyleRule(".test", nodes);

        rule1.ShouldBe(rule2);

        // Different nodes should create different rules
        var differentNodes = ImmutableList.Create<AstNode>(new Declaration("color", "blue"));
        var rule3 = new StyleRule(".test", differentNodes);
        rule1.ShouldNotBe(rule3);
    }

    [Fact]
    public void AtRule_Should_Create_With_Name_Params_And_Nodes()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "red")
        );

        var atRule = new AtRule("@media", "screen and (min-width: 640px)", nodes);

        atRule.Name.ShouldBe("@media");
        atRule.Params.ShouldBe("screen and (min-width: 640px)");
        atRule.Nodes.Count.ShouldBe(1);
    }

    [Fact]
    public void Comment_Should_Create_With_Value()
    {
        var comment = new Comment("This is a test comment");

        comment.Value.ShouldBe("This is a test comment");
    }

    [Fact]
    public void Context_Should_Create_With_Metadata_And_Nodes()
    {
        var metadata = ImmutableDictionary<string, object>.Empty
            .Add("layer", "utilities")
            .Add("priority", 100);

        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "red")
        );

        var context = new Context(metadata, nodes);

        context.Metadata.Count.ShouldBe(2);
        context.Metadata["layer"].ShouldBe("utilities");
        context.Metadata["priority"].ShouldBe(100);
        context.Nodes.Count.ShouldBe(1);
    }

    [Fact]
    public void SourceLocation_Should_Track_Position()
    {
        var loc = new SourceLocation(10, 5, 100, 20);

        loc.Line.ShouldBe(10);
        loc.Column.ShouldBe(5);
        loc.Offset.ShouldBe(100);
        loc.Length.ShouldBe(20);
        loc.EndColumn.ShouldBe(25);
        loc.EndOffset.ShouldBe(120);
    }

    [Fact]
    public void AstNode_Should_Support_Source_Tracking()
    {
        var src = new SourceLocation(1, 0, 0, 10);
        var dst = new SourceLocation(5, 0, 50, 15);

        var decl = new Declaration("color", "red")
        {
            Src = src,
            Dst = dst
        };

        decl.Src.ShouldBe(src);
        decl.Dst.ShouldBe(dst);
    }
}