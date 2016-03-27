$(function() {
  $(document.body).prepend('<div id="loading"><span class="text">Loading...</span><div></div></div>');

  $('#presentation')
    .append('<div class="social-widget left desk">' +
      '<strong>Follow us</strong>' +
      '<br/>' +
      '<a href="#"><img title="Our website"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/telerik_academy.png" /></a>' +
      '<a href="#"><img title="Follow us"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/facebook.png" /></a>' +
      '<a href="#"><img title="Subscribe"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/youtube.png" /></a>' +
      '<a href="#"><img title="Add us to circles"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/google_plus.png" /></a>' +
      '<a href="#"><img title="Follow"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/linkedin.png" /></a>' +
      '<a href="#"><img  title="Follow" src="https://rawgit.com/TelerikAcademy/Common/master/icons/twitter.png" /></a>' +
      '<a href="#"><img title="Subscribe"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/rss.png" /></a>' +
      '</div>');

  $('#presentation')
    .append('<div class="social-widget right desk">' +
      '<a href="#"><img class="longer" title="Follow us"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/facebook_like.png" /></a>' +
      '<a href="#"><img class="longer" title="Follow us"  src="https://rawgit.com/TelerikAcademy/Common/master/icons/facebook_share.png" /></a>' +
      '</div> ');


  $('.social-widget.mobile').addClass('hidden');

  $('.social-widget.mobile a').on('click', function(e){
    e.preventDefault();
    return false;
  });

  $('.social-widget.mobile').on('click', function() {
    $('.social-widget.mobile').toggleClass('hidden');
  });
});
