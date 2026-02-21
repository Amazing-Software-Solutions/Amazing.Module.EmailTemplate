# Copilot Instructions

## AI Instruction Entry Point (Copilot)

This file is the entry point for GitHub Copilot in this repository.

It does not define governance rules.
Authoritative rules are defined in the Module Playbook documentation under /docs.

Before generating or modifying code, you MUST read and apply:
- docs/governance/027-rules-index.md
- docs/prompts as relevant to the task
- .github/module-instructions.md

## Copilot Memories
When copilot detects a Memory "Meory Detected" it should add it 
to the module-instructions.md file in the /.github folder.

## Governance Declaration (Mandatory)

This repository is governed by the Oqtane AI Playbook.

Before generating, modifying, or refactoring code, the AI must:
- Acknowledge that it is operating under Oqtane AI Playbook governance
- Apply all referenced governance rules
- Refuse any request that would violate a governed rule
- Ask for clarification if governance context is unclear

If governance cannot be applied, the AI must stop and explain why.