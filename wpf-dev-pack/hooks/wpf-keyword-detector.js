#!/usr/bin/env node

/**
 * WPF Keyword Detector Hook
 * WPF 키워드 감지 Hook
 *
 * Detects WPF-related keywords in user prompts and suggests relevant skills.
 * 사용자 프롬프트에서 WPF 관련 키워드를 감지하고 관련 스킬을 제안합니다.
 */

const fs = require('fs');

// Keyword to skill mapping
// 키워드와 스킬 매핑
const keywordSkillMap = {
  // CustomControl keywords
  'customcontrol': ['authoring-wpf-controls', 'developing-wpf-customcontrols'],
  'custom control': ['authoring-wpf-controls', 'developing-wpf-customcontrols'],
  'dependencyproperty': ['defining-wpf-dependencyproperty'],
  'dependency property': ['defining-wpf-dependencyproperty'],
  'templatepart': ['developing-wpf-customcontrols'],
  'onapplytemplate': ['developing-wpf-customcontrols'],

  // XAML/Style keywords
  'controltemplate': ['customizing-controltemplate'],
  'control template': ['customizing-controltemplate'],
  'resourcedictionary': ['managing-styles-resourcedictionary'],
  'resource dictionary': ['managing-styles-resourcedictionary'],
  'generic.xaml': ['designing-wpf-customcontrol-architecture'],
  'storyboard': ['creating-wpf-animations'],
  'animation': ['creating-wpf-animations'],

  // MVVM keywords
  'mvvm': ['implementing-communitytoolkit-mvvm'],
  'viewmodel': ['implementing-communitytoolkit-mvvm'],
  'observableproperty': ['implementing-communitytoolkit-mvvm'],
  'relaycommand': ['implementing-communitytoolkit-mvvm'],
  'collectionview': ['managing-wpf-collectionview-mvvm'],
  'datatemplate': ['mapping-viewmodel-view-datatemplate'],

  // Rendering keywords
  'drawingcontext': ['rendering-with-drawingcontext'],
  'drawing context': ['rendering-with-drawingcontext'],
  'drawingvisual': ['rendering-with-drawingvisual'],
  'drawing visual': ['rendering-with-drawingvisual'],
  'onrender': ['rendering-with-drawingcontext'],
  'invalidatevisual': ['rendering-with-drawingcontext'],

  // Performance keywords
  'virtualizingstackpanel': ['virtualizing-wpf-ui'],
  'virtualizing': ['virtualizing-wpf-ui'],
  'freeze': ['optimizing-wpf-memory'],
  'freezable': ['optimizing-wpf-memory'],
  'bitmapcache': ['rendering-wpf-high-performance'],
  'performance': ['rendering-wpf-high-performance', 'optimizing-wpf-memory'],

  // Other WPF keywords
  'adorner': ['implementing-wpf-adorners'],
  'dragdrop': ['implementing-wpf-dragdrop'],
  'drag and drop': ['implementing-wpf-dragdrop'],
  'routed event': ['routing-wpf-events'],
  'routedevent': ['routing-wpf-events'],
  'command binding': ['handling-wpf-input-commands'],
  'inputbinding': ['handling-wpf-input-commands'],
  'flowdocument': ['creating-wpf-flowdocument'],
  'dialog': ['creating-wpf-dialogs'],
  'messagebox': ['creating-wpf-dialogs'],
  'clipboard': ['using-wpf-clipboard'],
  'localization': ['localizing-wpf-applications'],
  'automation': ['implementing-wpf-automation'],
  'mediaelement': ['integrating-wpf-media'],
  'visualtree': ['navigating-visual-logical-tree'],
  'logicaltree': ['navigating-visual-logical-tree'],
  'dispatcher': ['threading-wpf-dispatcher']
};

function detectKeywords(prompt) {
  const lowerPrompt = prompt.toLowerCase();
  const detectedSkills = new Set();

  for (const [keyword, skills] of Object.entries(keywordSkillMap)) {
    if (lowerPrompt.includes(keyword)) {
      skills.forEach(skill => detectedSkills.add(skill));
    }
  }

  return Array.from(detectedSkills);
}

function main() {
  try {
    // Read prompt from stdin or environment
    // stdin 또는 환경 변수에서 프롬프트 읽기
    const prompt = process.env.CLAUDE_USER_PROMPT || '';

    if (!prompt) {
      process.exit(0);
    }

    const detectedSkills = detectKeywords(prompt);

    if (detectedSkills.length > 0) {
      console.log(`[WPF Dev Pack] Detected relevant skills: ${detectedSkills.join(', ')}`);
    }

    process.exit(0);
  } catch (error) {
    console.error(`[WPF Dev Pack] Error: ${error.message}`);
    process.exit(0); // Don't block on error
  }
}

main();
