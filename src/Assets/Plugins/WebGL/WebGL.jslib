mergeInto(LibraryManager.library, {

	Play: function(stream, streamType){
		if(webGLPlayer.isDisposed()){
			createWebGLPlayerVideoTag();
			webGLPlayer = videojs('webGLPlayer');
			
			webGLPlayer.on("ended", function getNextSong() {
				myGameInstance.SendMessage("Bridge", "Next");
			});
		}
		
		webGLPlayer.src({type: UTF8ToString(streamType), src: UTF8ToString(stream)});
		webGLPlayer.play();
	},
	
	Resume: function(){
		webGLPlayer.play();
	},
	
	Pause: function(){
		webGLPlayer.pause();
	},
	
	GatherStatisticsData: function(reason){
		sendStatisticsData(reason);
	},
	
	SetPlayerVolume: function(volume){
		webGLPlayer.volume(volume);
	},

	InitRadioPlayer: function(){
		if(webGLPlayer instanceof videojs && !webGLPlayer.isDisposed()){
			webGLPlayer.dispose();
		}
		
		createWebGLPlayerVideoTag();
		webGLPlayer = videojs('webGLPlayer');
		
		webGLPlayer.on("ended", function getNextSong() {
			myGameInstance.SendMessage("Bridge", "Next");
		});
	},
	
	DisposeRadioPlayer: function(){
		webGLPlayer.dispose();
	},
});