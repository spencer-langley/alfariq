$(function () {

    String.prototype.format = String.prototype.f = function () {
        var s = this,
			i = arguments.length;

        while (i--) {
            s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
        }
        return s;
    };

    var timeoutIDs = [];

	var reader = new FileReader();
	var configFile = [];
	var XMLdata = [];
	var AdminPanelPos = "down";
	var sessionReady = false;
	var User = [];

	var Condition = [];
	//var TrialsPerBlock = 0;
	var OrderedOutcomes = [];
	var Words = [];
	var Profiles = {};
	var TrialBlockDurations = [];
	var StimulusDelay = 0;
	var FeedbackDelay = 0;
	var PositiveFeedbackMessage = '';
	var NegativeFeedbackMessage = '';
	//var GroupingOrder = [];
	var TrialBlocksPerSession = 8;
	var TrialBlockTimeout = 0;
	var KillIt = false;
	var NegTrialBlockFeedbackMessage = ''
	var PosTrialBlockFeedbackMessage = ''
	var SpecTrialBlockFeedbackMessage = ''
	var TrialBlockIntroMessage = ''
	var EndOfSessionMessage = '';
	var BonusDollarValue = 0.0;
	var CorrectCountGoal = 0;
	var MemberPreviewTime = 0;
	var ProfilesPerRow = 0;
	var TotalBonusEarned = 0;
	var CurrentCounterSecs = 0;
	
	var CurrentWordsDisplayed = [];
	var CurrentTranslationsClicked = [];
	var CurrentCorrectChoices = [];
	var CurrentCorrectCount = 0;
	var CurrentIncorrectChoices = [];
	var CurrentIncorrectCount = 0;
	var CompletedTrialBlocks = [];
	var CurrentTrialIndex = 0;
	var CurrentTrialBlockIndex = 0;
	var CurrentWrongWord1 = {};
	var CurrentWrongWord2 = {};
	var CurrentBlockStartTime = 0;
	var CurrentBlockEndTime = 0;
	var CurrentTrialDisplayTime = 0;
	var CurrentTrialSelectTime = 0;
	var CurrentBlockLatencies = [];
	var CurrentOptionsDisplayed = [];
	
	$('#BlockEndButton').click( BlockEndButtonClicked );
	$('.loginControls').prop('disabled', true);
	$('#loadFileButton').prop('disabled', true);
	$('#BlockEndButton').prop('disabled', true);
	$('.startHide').hide();
	$('.startHide').prop('disabled', true);
	$('#BeginTrialBlockButton').click( BeginTrialBlockButtonClick );
	$('.OptionContainer').click( SelectOption );
	
	Condition = $('#feedbackCondition').val();
	EndOfSessionMessage = $('#EndOfSessionMessage').val();

	if (Condition == "No Member Preview") {
	    TrialBlockIntroMessage = { single: $('#NMP-single-tbintro').val(), plural: $('#NMP-plural-tbintro').val() };
	    NegTrialBlockFeedbackMessage = "Your group did not meet their goal and no one will receive a bonus.";
	    PosTrialBlockFeedbackMessage = "Your group met their goal and each member will receive a bonus.";
	}
	else if (Condition == "Member Preview") {
	    TrialBlockIntroMessage = { single: $('#MP-single-tbintro').val(), plural: $('#MP-plural-tbintro').val() };
	    NegTrialBlockFeedbackMessage = "Your group did not meet their goal and no one will receive a bonus.";
	    PosTrialBlockFeedbackMessage = "Your group met their goal and each member will receive a bonus.";
	}
	else {
	    TrialBlockIntroMessage = { single: $('#MPSF-single-tbintro').val(), plural: $('#MPSF-plural-tbintro').val() };
	    NegTrialBlockFeedbackMessage = "Of the {0} members in your group, {1} met their goal and {2} did not. Therefore, no one in your group will receive a bonus.";
	    PosTrialBlockFeedbackMessage = "Of the {0} members in your group, {1} met their goal. Therefore, each member in your group will receive a bonus.";
	}

	SpecTrialBlockFeedbackMessage = " members did not meet the goal.  You did not earn a bonus.";
	TrialBlockTimeout = 120000;
	StimulusDelay = 1000;
	FeedbackDelay = 2500;
	PositiveFeedbackMessage = "Correct";
	NegativeFeedbackMessage = "Incorrect";
	BonusDollarValue = parseFloat(0.5);
	CorrectCountGoal = 13;
	MemberPreviewTime = 300;
	ProfilesPerRow = 6;
	//TrialsPerBlock = 120;
	sessionReady = true;
    Words = $.parseJSON($('#allWords').val());
    tempProfiles = $.parseJSON($('#allProfiles').val());

    for (var i = 0; i < tempProfiles.length; i++) {
        Profiles["id" + tempProfiles[i].Id] = tempProfiles[i];
    }

    

    function GroupSizeForTrialBlockIndex(i) {
        console.log('TBI: ' + i);
        console.log(OrderedOutcomes);
        return OrderedOutcomes[i].Fails.length + OrderedOutcomes[i].Passes.length + 1;
    }

    for (var i = 0; i < TrialBlocksPerSession; i++) {
        var passIdsKey = "#tb-" + i + "-passIDs";
        var failIdsKey = "#tb-" + i + "-failIDs";

        var passIds = $(passIdsKey).val().split(",");
        var failIds = $(failIdsKey).val().split(",");

        if (passIds == "") {
            passIds = [];
        }

        if (failIds == "") {
            failIds = [];
        }

        OrderedOutcomes.push({ "Passes": passIds, "Fails": failIds });
    }

    

    function TimeoutTrialBlock(tbi) {
        clearTimeout(TimeoutTrialBlock);
        if (KillIt) {
            return;
        }
        var timeoutID = setTimeout(function () {
            if (KillIt) {
                return;
            }
			if(CurrentTrialBlockIndex <= tbi)
			{
			    clearTimeout(TimeoutTrialBlock);
				console.log('Timed out TrialBlock!');
				KillIt = true;
				$('.OptionContainer').prop('disabled', true);
				$('#OptionsScreen').hide();
				EndTrialBlock();
			}
        }, TrialBlockTimeout);

        timeoutIDs.push(timeoutID);
	}
	
	function ClickShowResults() {
		var ResultsMarkup = '<table class=\'data-table\'><tr class=\'data-table\'><td class=\'data-table\'>User: ' + User + '</td><td class=\'data-table\'>Condition: ' + Condition + '</td></tr><td>Total Bonus Earned: $' + TotalBonusEarned.toFixed(2) + '</table><br />';
		for(var i = 0; i < CompletedTrialBlocks.length; i++)
		{
		    ResultsMarkup = ResultsMarkup + '<table class=\'data-table\'><tr class=\'data-table\'><td class=\'data-table\'>Trialblock: ' + i + '</td><td>Outcome: ' + OrderedOutcomes[i] + '</td><td class=\'data-table\'>Grouping: ' + GroupSizeForTrialBlockIndex(i) + '</td><td class=\'data-table\'></td></tr>';
			if(CompletedTrialBlocks[i].CorrectChoices.length >= CorrectCountGoal && OrderedOutcomes[i] == "Pass")
			{
				ResultsMarkup = ResultsMarkup + '<tr><td colspan=4>EARNED BONUS OF $' + BonusDollarValue.toFixed(2) + '</td></tr>'; 
			}
			else if(CompletedTrialBlocks[i].CorrectChoices.length >= CorrectCountGoal)
			{
				ResultsMarkup = ResultsMarkup + '<tr><td colspan=4>Goal met with no bonus awarded.</td></tr>'; 
			}
			ResultsMarkup = ResultsMarkup + '<tr><td colspan=4>TrialBlock Duration: ' + CompletedTrialBlocks[i].Duration/1000 + ' sec.</td></tr>';
			ResultsMarkup = ResultsMarkup + '<tr class=\'data-table\'><td class=\'data-table\'>Trial</td><td class=\'data-table\'>Presented</td><td>Clicked</td><td>Latency</td></tr>';
			for(var j = 0; j < CompletedTrialBlocks[i].TranslationsClicked.length; j++)
			{
				ResultsMarkup = ResultsMarkup + '<tr class=\'data-table\'><td class=\'data-table\'>' + j + '</td><td class=\'data-table\'>' + CompletedTrialBlocks[i].WordsDisplayed[j].Arabic + '</td><td>' + CompletedTrialBlocks[i].TranslationsClicked[j] + '</td><td>' + CompletedTrialBlocks[i].Latencies[j]/1000 + '</td></tr>';
			}
			ResultsMarkup = ResultsMarkup + '</table><br />';
		}
		var w = window.open();
		$(w.document.body).html(ResultsMarkup);
	}
	
	function EndSession() {

	    var submitData =
            {
                "TrialBlocks": CompletedTrialBlocks,
                "SessionId" : $("#sessionID").val()
            }

	    $.ajax({
	        type: "POST",
	        url: "/Home/SaveSession",
	        data: JSON.stringify( submitData ),
	        contentType: "application/json",
	        dataType: "json",
	        success: function (data) { alert(data.Message); },
	        failure: function (errMsg) {
	            alert(errMsg);
	        }
	    });

		$('#TrialScreen').html( EndOfSessionMessage + TotalBonusEarned.toFixed(2));
		$('#TrialScreen').show();
		$('.adminInput').prop('disabled', true);
		$('#loadFileButton').prop('disabled', true);
		$('#adminMessage').text('Click here to view results.');
		$('#adminMessage').click( ClickShowResults );
		$('#sliderButton').css('color', '#ff0000');
		$('#sliderButton').prop('disabled', false);
		console.log('End Session!');
		console.log(CompletedTrialBlocks);
	}
	
	function BlockEndButtonClicked() {
		$('#BlockEndScreen').hide();
		$('#BlockEndButton').prop('disabled', true);
		$('#BlockEndScreenMessage').html('');
		if(CurrentTrialBlockIndex < TrialBlocksPerSession)
		{
			StartTrialBlock();
		}
		else
		{
			EndSession();
		}
	}
	
	function SpecOutcomesByGroupSizeAndOutcome(size, outcomeRate) {
		var out_array = [];
		var fails = 0;
		if(outcomeRate == 1)
		{
			fails = 1;
		}
		else
		{
			fails = size*outcomeRate;
		}
		for(var i = 0; i < size; i++)
		{
			out_array.push( (i >= fails) );
		}
		return Shuffle(out_array);
	}
	
	function getFailCount(hitTarget) {
		var selfCount = 0;
		if(!hitTarget)
		{
			selfCount++;
		}
		return OrderedOutcomes[CurrentTrialBlockIndex].Fails.length + selfCount;
	}
	
	function PresentSpecificOutcomes(userCorrect) {
	    var bonus = userCorrect;
	    var combinedIDs = [];
	    for (var i = 0; i < OrderedOutcomes[CurrentTrialBlockIndex].Passes.length; i++) {
	        combinedIDs.push(OrderedOutcomes[CurrentTrialBlockIndex].Passes[i]);
	    }
	    for (var i = 0; i < OrderedOutcomes[CurrentTrialBlockIndex].Fails.length; i++) {
	        combinedIDs.push(OrderedOutcomes[CurrentTrialBlockIndex].Fails[i]);
	    }

	    combinedIDs.push(-1); //user
	    var shuffledProfileIDs = Shuffle(combinedIDs);

	    console.log('Shuffled IDs:');
	    console.log(shuffledProfileIDs);

	    var previewMarkup = '<center><table>';
	    var memberCount = GroupSizeForTrialBlockIndex(CurrentTrialBlockIndex);
	    var i;
	    var currentProfile;
	    console.log(memberCount);
	    console.log(shuffledProfileIDs);
	    for (i = 0; i < memberCount; i++) {
	        currentProfile = Profiles["id" + shuffledProfileIDs[i]];
	        if (i % ProfilesPerRow == 0) {
	            previewMarkup += '<tr>';
	        }
	        var passThisProfile = PassOrFailById(shuffledProfileIDs[i], userCorrect);
	        bonus = bonus && passThisProfile;
	        if (passThisProfile) {
	            previewMarkup += '<td><img src=\'' + currentProfile.ImagePath + '\'><br/>' + currentProfile.Name + '<br/> Pass </td>';
	        }
	        else {
	            previewMarkup += '<td><img src=\'' + currentProfile.ImagePath + '\'><br/>' + currentProfile.Name + '<br/> Fail </td>';
	        }
	        if (i % ProfilesPerRow == (ProfilesPerRow - 1)) {
	            previewMarkup += '</tr>';
	        }
	    }
	    if (i % ProfilesPerRow != (ProfilesPerRow - 1)) {
	        previewMarkup += '</tr>';
	    }
	    previewMarkup += '</table>';

	    if (bonus) {
	        previewMarkup += 'Your group will receive a bnous.</center>';
	        TotalBonusEarned += BonusDollarValue;
	    }
	    else {
	        previewMarkup += 'Your group will not receive a bnous.</center>';
	    }

	    $('#blockIntroScreen').hide();
	    $('#BlockEndScreenMessage').html(previewMarkup);
	    $('#BlockEndScreen').show();
	}

	function PassOrFailById(id, userCorrect) {
	    if (id == -1) return userCorrect;
	    for (var i = 0; i < OrderedOutcomes[CurrentTrialBlockIndex].Fails.length; i++) {
	        if (OrderedOutcomes[CurrentTrialBlockIndex].Fails[i] == id) {
	            return false;
	        }
	    }
	    return true;
	}

	function PresentTrialBlockResults() {

	    var hitTarget = false;
	    if (CompletedTrialBlocks[CurrentTrialBlockIndex].CorrectChoices.length >= CorrectCountGoal) {
	        hitTarget = true;
	    }

	    $('#CountdownContainer').html('');
	    $('#TrialCounterContainer').html('');

		if (Condition == 'Specific Feedback')
		{
			PresentSpecificOutcomes(hitTarget);
		}
		else
		{
			if(OrderedOutcomes[CurrentTrialBlockIndex].Fails.length == 0 && hitTarget)
			{
				$('#BlockEndScreenMessage').append( PosTrialBlockFeedbackMessage );
				TotalBonusEarned += BonusDollarValue;
			}
			else
			{
				$('#BlockEndScreenMessage').append( NegTrialBlockFeedbackMessage );
			}
		}
		$('#BlockEndButton').prop('disabled', false);
		CurrentTrialBlockIndex++;
		$('#BlockEndScreen').show();
	}
	
	function EndTrialBlock() {
	    FlushTimeouts();
		console.log('Ending trial block');
		$('#TrialScreen').hide();
		CurrentBlockEndTime = Date.now();
		CompletedTrialBlocks.push({
		    WordsDisplayed: CurrentWordsDisplayed,
		    OptionsDisplayed: CurrentOptionsDisplayed,
			TranslationsClicked: CurrentTranslationsClicked,
			CorrectChoices: CurrentCorrectChoices,
			Latencies: CurrentBlockLatencies,
			Duration: (CurrentBlockEndTime - CurrentBlockStartTime)
		});
		
		CurrentBlockEndTime = 0;
		CurrentBlockStartTime = 0;
		CurrentBlockLatencies = [];
		CurrentTrialIndex = 0;
		CurrentTrialDisplayTime = 0;
		CurrentTrialSelectTime = 0;
		CurrentWordsDisplayed = [];
		CurrentTranslationsClicked = [];
		CurrentCorrectChoices = [];
		CurrentOptionsDisplayed = [];
		PresentTrialBlockResults();
	}
	
	function EndTrial() {
		$('#TrialScreen').removeClass('incorrect correct');
		if(KillIt)
		{
			Die();
			return;
		}
		console.log('Ending trial ' + CurrentTrialIndex);
		CurrentTrialIndex++;
	    //if(CurrentTrialIndex < TrialsPerBlock && CurrentCorrectCount < CorrectCountGoal)
		if (CurrentCorrectCount < CorrectCountGoal)
		{
			ConductTrial();
		}
		else
		{
		    Die();
		    clearTimeout(TimeoutTrialBlock);
			EndTrialBlock();
		}
	}
	
	function PresentFeedback(userCorrect) {
	    RefreshTrialCounter();
		$('#OptionsScreen').hide();
		if(KillIt)
		{
			Die();
			return;
		}
		if(userCorrect)
		{
			CurrentCorrectCount = CurrentCorrectCount + 1;
			$('#TrialScreen').addClass('correct').html(PositiveFeedbackMessage);
		}
		else
		{
			CurrentIncorrectCount = CurrentIncorrectCount + 1;
			$('#TrialScreen').addClass('incorrect').html(NegativeFeedbackMessage);
		}
		console.log('Waiting ' + FeedbackDelay + 'ms');
		$('#TrialScreen').show();
		timeoutIDs.push( setTimeout(EndTrial, FeedbackDelay));
	}
	
	function SelectOption() {
		CurrentTrialSelectTime = Date.now();
		$('.OptionContainer').prop('disabled', true);
		CurrentTranslationsClicked.push($(this).text());
		console.log('RS: ' + CurrentTranslationsClicked[CurrentTrialIndex]);
		console.log('LS: ' + CurrentWordsDisplayed[CurrentTrialIndex].English);
		CurrentBlockLatencies.push(CurrentTrialSelectTime - CurrentTrialDisplayTime);
		CurrentTrialSelectTime = 0;
		CurrentTrialDisplayTime = 0;
		if(CurrentTranslationsClicked[CurrentTrialIndex] == CurrentWordsDisplayed[CurrentTrialIndex].English)
		{
			console.log('Good choice');
			CurrentCorrectChoices.push(CurrentWordsDisplayed[CurrentTrialIndex]);
			PresentFeedback(true);
		}
		else
		{
			console.log('Bad choice');
			PresentFeedback(false);
		}
	}
	
	function DisplayOptions() {
		if(KillIt)
		{
			Die();
			return;
		}
		$('.OptionContainer').prop('disabled', false);
		var displayWords = [ CurrentWordsDisplayed[CurrentTrialIndex], CurrentWrongWord1, CurrentWrongWord2 ];
		displayWords = Shuffle(displayWords);
		CurrentOptionsDisplayed.push(displayWords);
		$('#Option1').html(displayWords[0].English);
		$('#Option2').html(displayWords[1].English);
		$('#Option3').html(displayWords[2].English);
		//$('#TrialScreen').hide();
		$('#OptionContainer').show();
		$('#OptionsScreen').show();
		CurrentTrialDisplayTime = Date.now();
	}
	
	function ConductTrial() {
		if(KillIt)
		{
			Die();
			return;
		}
		RefreshTrialCounter();
		console.log('Conduct trial ' + CurrentTrialIndex);
		var shuffledWords = Shuffle(Words);
		CurrentWordsDisplayed.push(shuffledWords[0]);
		CurrentWrongWord1 = shuffledWords[1];
		CurrentWrongWord2 = shuffledWords[2];
		$('#TrialScreen').html(shuffledWords[0].Arabic);
		console.log('Waiting ' + StimulusDelay + 'ms');
		$('#TrialScreen').show();
		timeoutIDs.push(setTimeout(DisplayOptions, StimulusDelay));
	}
	
	function Die() {
	    //KillIt = false;
	    $('#CountdownContainer').html('');
	    $('#TrialCounterContainer').html('');
	    CurrentCounterSecs = 0;
	    $('#CountdownContainer').hide();
	    clearTimeout(CounterGo);
		//CurrentBlockEndTime = Date.now();
	}
	
	function FinishTrialIntro() {
	    clearTimeout(FinishTrialIntro);
		$('#blockIntroScreen').hide();
		TimeoutTrialBlock(CurrentTrialBlockIndex);
		CurrentBlockStartTime = Date.now();
		clearTimeout(CounterGo);
		console.log('Starting timer');
		CurrentCounterSecs = 0;
		CounterGo();
		console.log('Timer started');
		$('#CountdownContainer').show();
		ConductTrial();
	}
	
	function BeginTrialBlockButtonClick() {
		if( displayMemberPreview() )
		{
		    console.log('Display member preview');
			memberPreview();
		}
		else
		{
		    console.log('No member preview');
			FinishTrialIntro();
		}
	}
	
	function memberPreview() {

	    var combinedIDs = [];
	    for (var i = 0; i < OrderedOutcomes[CurrentTrialBlockIndex].Passes.length; i++) {
	        combinedIDs.push(OrderedOutcomes[CurrentTrialBlockIndex].Passes[i]);
	    }
	    for (var i = 0; i < OrderedOutcomes[CurrentTrialBlockIndex].Fails.length; i++) {
	        combinedIDs.push(OrderedOutcomes[CurrentTrialBlockIndex].Fails[i]);
	    }

	    combinedIDs.push(-1); //user
	    var shuffledProfileIDs = Shuffle(combinedIDs);

	    console.log('Shuffled IDs:');
	    console.log(shuffledProfileIDs);

		var previewMarkup = '<center><table>';
		var memberCount = GroupSizeForTrialBlockIndex(CurrentTrialBlockIndex);
		var i;
		var currentProfile;
		console.log(memberCount);
		console.log(shuffledProfileIDs);
		for(i = 0; i < memberCount; i++)
		{
		    currentProfile = Profiles["id" + shuffledProfileIDs[i]];
			if(i % ProfilesPerRow == 0)
			{
				previewMarkup += '<tr>';
			}
			previewMarkup += '<td><img src=\'' + currentProfile.ImagePath + '\'><br/>' + currentProfile.Name + '<br/>' + currentProfile.Age + ' ' + currentProfile.Home + '</td>';
			if(i % ProfilesPerRow == (ProfilesPerRow-1))
			{
				previewMarkup += '</tr>';
			}
		}
		if(i % ProfilesPerRow != (ProfilesPerRow-1))
		{
			previewMarkup += '</tr>';
		}
		previewMarkup += '</table></center>';
		$('#blockIntroScreen').hide();
		$('#TrialScreen').html(previewMarkup);
		$('#TrialScreen').show();
		console.log('setting timeout...');
		timeoutIDs.push(setTimeout(FinishTrialIntro, MemberPreviewTime*memberCount));
	}
	
	function StartTrialBlock() {
		$('#TrialScreen').html('');
		$('#TrialScreen').hide();
		KillIt = false;
		console.log('Start TrialBlock ' + CurrentTrialBlockIndex);
		var introMessage = '';
		console.log('Group size: ' + GroupSizeForTrialBlockIndex(CurrentTrialBlockIndex));
		if (GroupSizeForTrialBlockIndex(CurrentTrialBlockIndex) == 2)
		{
		    introMessage = TrialBlockIntroMessage.single;
		    console.log('Single message');
		}
		else
		{
		    introMessage = TrialBlockIntroMessage.plural;
		    console.log('Plural message');
		}
		console.log('Message: ' + introMessage);
		introMessage = introMessage.format(GroupSizeForTrialBlockIndex(CurrentTrialBlockIndex), CorrectCountGoal);
		$('#blockIntroMessage').text(introMessage);
		$('#blockIntroMessage').show();
		$('#blockIntroScreen').show();
		CurrentTrialIndex = 0;
		CurrentCorrectCount = 0;
		CurrentIncorrectCount = 0;
	}

	function RefreshTrialCounter() {
	    $('#TrialCounterContainer').html(CurrentCorrectChoices.length + ' of ' + CurrentTranslationsClicked.length);
	}

	function CounterGo() {
	    clearTimeout(CounterGo);
	    if (!KillIt) {
	        $('#CountdownContainer').html(SecondsToMinutes((TrialBlockTimeout / 1000) - CurrentCounterSecs));
	        CurrentCounterSecs++;
	        timeoutIDs.push(setTimeout(CounterGo, 1000));
	    }
	    else {
	        clearTimeout(CounterGo);
	        $('#CountdownContainer').html('');
	        $('#TrialCounterContainer').html('');
	        CurrentCounterSecs = 0;
	    }
	}

	function SecondsToMinutes(secs) {
	    var mins = Math.floor(secs / 60);
	    //var remainingSecs = secs - (mins * 60);
	    var remainingSecs = secs % 60;
	    if (remainingSecs < 10) {
	        remainingSecs = "0" + remainingSecs;
	    }
	    return mins + ':' + remainingSecs;
	}
	
	//function StartSession() {
	//	$('#userScreen').html('');
	//	StartTrialBlock();
	//}
	
	function displayMemberPreview() {
		return (Condition == 'Member Preview' || Condition == 'Specific Feedback');
	}
	
	function Shuffle(source_array) {
	    var arr = [];

	    for (var i = 0; i < source_array.length; i++) {
	        arr.push(source_array[i]);
	    }

	    for (var j, x, i = arr.length; i; j = parseInt(Math.random() * i), x = arr[--i], arr[i] = arr[j], arr[j] = x);

	    return arr;
	};

	function FlushTimeouts() {
	    while (timeoutIDs.length > 0) {
	        clearTimeout(timeoutIDs.pop());
	    }
	}

	StartTrialBlock();
});