var render = (function () {

  function removeHidden(markdown) {
    // var regex = /<!--\s*attr\s*:\s*{.+showInPresentation.+}\s-->\s*(<!--.*-->)/gi;
    var regex = /<!--\s*attr\s*:\s*{.*showInPresentation.*}\s-->\s*(<!--[\s\S]+?-->)/gi;
    markdown = markdown.replace(regex, function (whole, group) {
      var fixed = group.substring('<!--'.length);
      fixed = fixed.substring(0, fixed.length - '-->'.length);
      return whole.replace(group, fixed).trim();
    });
    return markdown;
  }

  function sectionStringToSlides(sectionString) {
    var lines = sectionString.split('\n'),
      slides = [],
      slide = '';
    lines.forEach(function (line) {
      var trimmedLine = line.trim();
      if (trimmedLine.indexOf('# ') === 0 ||
        trimmedLine.indexOf('#\t') === 0 ||
        trimmedLine.indexOf('attr:') >= 0) {
        if (slide.trim() !== '') {
          slides.push(slide);
        }
        slide = '';
      }
      slide += line;
      slide += '\n';
    });
    if (slide.trim() !== '') {
      slides.push(slide);
    }
    return slides;
  }

  function parseMarkdown(markdown) {
    var sectionsStrings = markdown.trim().split(/<!--[ ]+section start[ ]+-->/g);
    var sections = [];

    sectionsStrings.forEach(function (sectionString) {
      var slides = sectionStringToSlides(sectionString);
      sections.push({
        slides: slides
      });
    });
    return sections;
  }

  function fillSections(sections) {
    'use strict';
    var $sectionsContainer = $("<div/>");
    sections.forEach(function (section, index) {
      if (!section.slides || !section.slides.length || section.slides.every(function (slide) {
          return slide.trim() === '';
        })) {
        return;
      }

      var $masterSection = $('<section />')
        .appendTo($sectionsContainer);

      var attr = {};
      section.slides.forEach(function (slide) {
        slide = slide.trim();
        if (slide.indexOf('<!-- attr: ') >= 0) {
          var fromIndex = slide.indexOf('<!-- attr:'),
            toIndex = slide.indexOf('}', fromIndex + 1),
            attrString = slide.substring(fromIndex + '<!-- attr:'.length, toIndex + 1);
          eval('attr=' + attrString);
          return;
        }

        var $section = $('<section/>')
          .attr('data-markdown', '')
          .appendTo($masterSection);
        if (attr.hasScriptWrapper) {
          slide = '<' + 'script type="text/md-tmp">' +
            slide +
            '</' + 'script>';
        }

        $section.html(slide);

        for (var attrName in attr) {
          $section.attr(attrName, attr[attrName]);
        }
        attr = {};
      });
    });
    $('#slides-container').html($sectionsContainer.html());
  }

  function render(filename) {
    $.ajax(filename, {
      success: function (markdown) {
        markdown = removeHidden(markdown);
        console.log(markdown);
        var sections = parseMarkdown(markdown);
        fillSections(sections);
        setupRevealJs();
      },
      error: function () {

      }
    });
  }

  return render;
}());
