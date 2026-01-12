# Let It Ralph: Building a Git Commit Analyser with Claude Code's Autonomous Loop Plugin

_How I used the Ralph Wiggum technique to build a complete C# tool overnight_

---

## The Problem with One-Shot AI Coding

If you've used AI coding assistants, you know the drill: prompt, wait, review, tweak, prompt again. It's helpful, but it's still fundamentally a back-and-forth dance. What if you could just tell the AI what to build and walk away?

Enter **Ralph Wiggum** — an official Claude Code plugin that turns one-off prompts into persistent development loops.

## What is the Ralph Wiggum Plugin?

Named after the lovably persistent Simpsons character, Ralph Wiggum embodies a simple philosophy: **keep trying until you succeed**.

The technique was created by Geoffrey Huntley, who described it simply:

> "Ralph is a Bash loop."

The plugin intercepts Claude Code's exit attempts, re-feeds your original prompt, and lets the AI iterate on its own work. Each cycle, Claude sees the modified files and git history from previous runs, learning from what worked and what didn't.

```bash
/ralph-loop "Build feature X" --max-iterations 20 --completion-promise "DONE"
```

That's it. One command, and Claude Code works autonomously until the job is done or the iteration limit is hit.

## The Project: A Git Commit Analyser

I wanted to build something practical — a C# console app that analyses a repository's commit history and displays beautiful statistics:

- Commit frequency by day and hour
- Most frequently edited files
- Author contribution breakdown
- Streak tracking

The kind of tool that's useful but tedious to build manually.

## Setting Up for Ralph

The key to successful Ralph loops is **good prompt engineering**. You need to give Claude clear success criteria and a structured roadmap.

### Step 1: Create Your Project Skeleton

Start with a minimal foundation:

```csharp
// Program.cs
using Spectre.Console;

namespace GitCommitAnalyser;

public class Program
{
    public static int Main(string[] args)
    {
        AnsiConsole.Write(
            new FigletText("Git Analyser")
                .Colour(Colour.Blue));

        // TODO: Features will be implemented by Ralph
        return 0;
    }
}
```

### Step 2: Write the PROMPT.md

This is the heart of the Ralph technique. Your prompt file tells Claude exactly what to build:

```markdown
# Git Commit Analyser - Ralph Loop Prompt

## Project Goal

Build a C# console application that analyses Git commit history.

## Requirements (Implement in Order)

1. **Command Line Interface**

   - Accept path argument
   - Add --days flag for date range
   - Use Spectre.Console.Cli

2. **Git Data Extraction**

   - Execute git log commands
   - Parse commits: hash, author, date, files

3. **Statistics Engine**

   - Commit frequency by day/hour
   - Most edited files (top 10)
   - Author contributions

4. **Visualisation**

   - ASCII charts with Spectre.Console
   - Colour-coded output

## Completion Signal

When ALL requirements pass, output:
<promise>ANALYSER_COMPLETE</promise>

## If Stuck

After 15 iterations without progress, document blockers and stop.
```

Notice the structure:

- **Clear ordering** — Claude knows what to tackle first
- **Completion signal** — tells Ralph when to stop
- **Escape hatch** — prevents infinite loops on unsolvable problems

### Step 3: Initialise Git

Ralph uses git history to track progress between iterations:

```bash
cd git-commit-analyser
git init
git add .
git commit -m "Initial scaffold for Ralph loop"
```

### Step 4: Install and Run Ralph

```bash
# Install the plugin
claude /plugin install ralph-wiggum@claude-plugins-official

# Start the loop
/ralph-loop "Build the Git Commit Analyser according to PROMPT.md" \
  --max-iterations 25 \
  --completion-promise "ANALYSER_COMPLETE"
```

Then... walk away. Make coffee. Go to bed.

## How Ralph Works Under the Hood

The plugin uses a **Stop hook** — a mechanism that intercepts Claude's exit:

1. Claude works on the task
2. Claude tries to exit (thinks it's done or stuck)
3. Stop hook catches the exit
4. Stop hook re-injects the original prompt
5. Claude sees its previous work via git log
6. Repeat until completion signal or iteration limit

The magic is in step 5. Each iteration, Claude reviews what it built, sees compile errors or test failures, and adjusts. It's essentially TDD without human intervention.

## Lessons Learned

### 1. Prompt Quality is Everything

Vague prompts lead to wandering iterations. Be specific about:

- Feature requirements
- File structure
- Success criteria
- What to do when stuck

### 2. Set Conservative Iteration Limits

Start with `--max-iterations 20`. You can always run another loop. A 50-iteration loop on a large codebase can cost $50+ in API credits.

### 3. Use Programmatic Success Criteria

The best Ralph loops have success conditions that Claude can verify:

- `dotnet build` succeeds
- Tests pass
- Specific files exist

Avoid subjective criteria like "make it look good."

### 4. Git History is Your Debug Log

Between iterations, check `git log --oneline` to see what Claude attempted. Failed approaches are still valuable data.

## When NOT to Use Ralph

Ralph excels at **mechanical tasks** with clear success criteria:

- ✅ Migrations
- ✅ Batch refactoring
- ✅ Test coverage expansion
- ✅ Documentation generation
- ✅ Boilerplate-heavy features

It struggles with **judgement-heavy work**:

- ❌ Architecture decisions
- ❌ UI/UX design
- ❌ Complex business logic without specs
- ❌ Anything requiring human feedback mid-process

## The Results

After 7 minutes, my Git Commit Analyser was complete:

- Full CLI with help documentation
- Parsing for all git log output formats
- Beautiful Spectre.Console charts
- Error handling for non-git directories

Larger projects can run for hours without any human intervention; however, please check your token usage.

![Ralph Final Output](https://t2560239.p.clickup-attachments.com/t2560239/01cc4391-1979-4b7a-982e-878421eb65c7/ralph-final.png)

## Getting Started

You can view the relevant files in the repository: <https://github.com/Markus243/GitCommitAnalyzer> — please go give it a star!

```text
git-commit-analyser/
├── GitCommitAnalyser.csproj
├── Program.cs          # Minimal starting point
├── PROMPT.md           # Your Ralph instructions
├── README.md
└── .gitignore
```

Clone it, customise the PROMPT.md for your needs, and let Ralph do the work.

## Testing

### Boardly

![Boardly](https://t2560239.p.clickup-attachments.com/t2560239/2f2032be-ebf2-4de9-9178-6034d3fa4486/boardly-stats.png)

As you can see, for an iterative, one-shot agentic build, the end result is awesome — it compiles the first time, and all of the requirements were met without error!

## The Philosophy

Geoffrey Huntley summarises it perfectly:

> "The technique is deterministically bad in an undeterministic world. It's better to fail predictably than succeed unpredictably."

Ralph shifts the skill from "directing Claude step by step" to "writing prompts that converge toward correct solutions." Your job becomes setting up conditions where iteration leads to success.

It's not magic. It's just persistence — the Ralph Wiggum way.

---

_Have you tried the Ralph Wiggum technique? Share your experiences in the comments!_

---

## Resources

- [Ralph Wiggum Plugin (Official)](https://github.com/anthropics/claude-code/tree/main/plugins/ralph-wiggum)
- [Claude Code Documentation](https://docs.anthropic.com/claude-code)
- [Geoffrey Huntley's Original Thread](https://twitter.com/geoffreyhuntley)

---

**Tags:** #ai #claudecode #dotnet #csharp #automation #devtools
