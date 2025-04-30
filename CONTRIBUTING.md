# 🤝 Before Contributing

Before making any contributions to this project, please make sure to **read and follow** the guidelines outlined below. This will help us maintain clean code, smooth collaboration, and consistent project structure.

---

## 🚀 Getting Started

### Initial Setup

To start working on the project:
```bash
git clone https://<your_token_id>@github.com/AlgorithmsNest/speaker-identification-system.git

cd speaker-identification-system
```

After cloning, remember to:
- Always pull the latest changes before you start working.

  ```bash
  git pull origin dev
  ```
  
- Push your changes after committing locally:
  ```bash
  git push origin your-branch-name
  ```

---

## 🧠 Workflow and Project Structure

### Branching Strategy

We follow a simple Git workflow with the following branches:
- `main` — production-ready code (no direct commits)
- `dev` — active development branch (pull requests should target this)

#### Feature Branches

Each new feature or fix must be developed in its own branch, branched off `dev`.

**Branch naming convention:**
```bash
git branch feature-name-with-dashes
```

Example:
- `pruning-search-paths`

---

### Pull Requests & Pair Programming

- Always work in **pair programming** mode.
- When opening a pull request:
  - Assign your pair partner as the reviewer.
  - They will either approve and merge into `dev` or request changes.

---

## 🗂️ Directory Structure & Documentation

- All documentation related to functions, logic, and prototypes should be written in **Markdown** and placed in the `docs/` directory.
- Keep code clean and well-commented.
- Write function headers or summaries for clarity.

---

## ✅ Coding Standards

- Stick to clean, readable C# code.
- Follow established naming conventions:
  - [C# Coding Standards & Naming Conventions](https://github.com/ktaranov/naming-convention/blob/master/C%23%20Coding%20Standards%20and%20Naming%20Conventions.md)

---

## 📄 Commit Messages

Use **conventional commit types** to keep commit history clear and organized.

```bash
git commit -m "type: short description" -m "Longer optional description"
```

Examples:
- `feat: add pruning algorithm`
- `fix: correct bug in voice matcher`
- `docs: update README with setup steps`

More info: [Git Conventional Commit Types](https://github.com/pvdlg/conventional-changelog-metahub?tab=readme-ov-file#commit-types)

---

## ✅ Before Making a Pull Request

- Ensure your code runs correctly and passes all tests.
- Verify the output is as expected.
- Write or update documentation.

---

Then you're ready to start coding!

## 📚 Useful Resources

- [Git Cheat Sheet (GitHub)](https://education.github.com/git-cheat-sheet-education.pdf)
- [Markdown Cheat Sheet](https://www.markdownguide.org/cheat-sheet/)
- [Review as if You Own the Code](https://www.red-gate.com/simple-talk/dotnet/.net-framework/the-zen-of-code-reviews-review-as-if-you-own-the-code/)

### 📘 Code Review Best Practices
- [Better Code Reviews with GIT](https://www.red-gate.com/simple-talk/dotnet/software-delivery/better-code-reviews-with-git/)

- [Part 1: Pre-Review Comments](https://www.red-gate.com/simple-talk/dotnet/.net-framework/the-zen-of-code-reviews-pre-review-comments/)

- [Part 2: Best Practices](https://www.red-gate.com/simple-talk/dotnet/.net-framework/the-zen-of-code-reviews-best-practices/)

- [Part 3: The Reviewer’s Tale](https://www.red-gate.com/simple-talk/dotnet/.net-framework/the-zen-of-code-reviews-the-reviewer's-tale/)

- [Part 4: Review as if You Own the Code](https://www.red-gate.com/simple-talk/dotnet/.net-framework/the-zen-of-code-reviews-review-as-if-you-own-the-code/)