#!/usr/bin/env node

/**
 * XAML Validator Hook
 * XAML 검증 Hook
 *
 * Validates XAML files after Write/Edit operations for common issues.
 * Write/Edit 작업 후 XAML 파일의 일반적인 문제를 검증합니다.
 */

const fs = require('fs');
const path = require('path');

// Common XAML issues to check
// 검사할 일반적인 XAML 문제들
const validationRules = [
  {
    name: 'Missing x:Key in Style without TargetType',
    pattern: /<Style\s+(?!.*x:Key)(?!.*TargetType)/i,
    message: 'Style should have either x:Key or TargetType attribute'
  },
  {
    name: 'Direct content in Generic.xaml',
    pattern: /<Style\s+.*TargetType.*(?<!MergedDictionaries)/i,
    checkFile: (filePath) => filePath.toLowerCase().includes('generic.xaml'),
    message: 'Generic.xaml should only contain MergedDictionaries, not direct styles'
  },
  {
    name: 'Missing xmlns:x namespace',
    pattern: /<ResourceDictionary(?![\s\S]*xmlns:x=)/i,
    message: 'ResourceDictionary should include xmlns:x namespace declaration'
  },
  {
    name: 'TemplateBinding outside ControlTemplate',
    pattern: /<(?!ControlTemplate)[\w.]+[^>]*TemplateBinding/i,
    message: 'TemplateBinding should only be used inside ControlTemplate'
  },
  {
    name: 'Missing PART_ prefix for template parts',
    pattern: /x:Name="(?!PART_)[A-Z][^"]*".*(?:ContentPresenter|Border|Grid)/i,
    message: 'Template parts should use PART_ prefix (e.g., PART_ContentHost)'
  }
];

function validateXaml(content, filePath) {
  const issues = [];

  for (const rule of validationRules) {
    // Skip if rule has file-specific check that doesn't match
    // 파일별 검사가 있고 매치되지 않으면 건너뜀
    if (rule.checkFile && !rule.checkFile(filePath)) {
      continue;
    }

    if (rule.pattern.test(content)) {
      issues.push({
        rule: rule.name,
        message: rule.message
      });
    }
  }

  return issues;
}

function main() {
  try {
    // Get file path from environment
    // 환경 변수에서 파일 경로 가져오기
    const filePath = process.env.CLAUDE_TOOL_FILE_PATH || '';

    if (!filePath) {
      process.exit(0);
    }

    // Only validate XAML files
    // XAML 파일만 검증
    const ext = path.extname(filePath).toLowerCase();
    if (ext !== '.xaml') {
      process.exit(0);
    }

    // Check if file exists
    // 파일 존재 여부 확인
    if (!fs.existsSync(filePath)) {
      process.exit(0);
    }

    const content = fs.readFileSync(filePath, 'utf-8');
    const issues = validateXaml(content, filePath);

    if (issues.length > 0) {
      console.log(`[WPF Dev Pack] XAML validation warnings for ${path.basename(filePath)}:`);
      issues.forEach(issue => {
        console.log(`  - ${issue.rule}: ${issue.message}`);
      });
    }

    process.exit(0);
  } catch (error) {
    console.error(`[WPF Dev Pack] Validation error: ${error.message}`);
    process.exit(0); // Don't block on error
  }
}

main();
