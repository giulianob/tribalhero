/*
*   The MIT License
*   
*   Copyright (c) 2009 Eric Decker
*   
*   Permission is hereby granted, free of charge, to any person obtaining a copy
*   of this software and associated documentation files (the "Software"), to deal
*   in the Software without restriction, including without limitation the rights
*   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*   copies of the Software, and to permit persons to whom the Software is
*   furnished to do so, subject to the following conditions:
*   
*   The above copyright notice and this permission notice shall be included in
*   all copies or substantial portions of the Software.
*   
*   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
*   THE SOFTWARE.
*/
 
package src.Util {
 
    import flash.utils.Dictionary;
    /**
     * @author eric decker : firstborn : 2009
     */
    public class ProfanityFilter {
        
        //high always matches, low maches only as a seperate word
        private var defaultListHigh:String = "fuck,nigger,bullshit,spacedick,spaceclop,clopclop";
        private var defaultListLow:String = "nigguh,nigga,cunt,shit,fag,faggot,goatse,lemonparty,clop,slut,twat,wetback,whore,nigglet";
        private var defaultIgnoreList:String ="crap";
        private var escaped:Array;
        private var letterAssociations:Array;
        private var formatedProfanityListHigh:Array;
        private var formatedProfanityListLow:Array;
        private var formatedIgnoreList:Array;
        private var rawProfanityListHigh:Array;
        private var rawProfanityListLow:Array;
        private var rawIgnoreList:Array;
        private var init:Boolean = false;
        private var foreignChars:Dictionary;
        private var foreignCharRegEx:RegExp;
         
        /**
         * creates a new filter
         * 
         */
        public function ProfanityFilter() {
        }
 
        /**
         * Validates a string for profanity
         *  @param text the text to test
         *  @param unescapeForeign whether to first remove foriegn/accented characters that otherwise cause errors with index positions
         *  
         *  @return returns a ProfanityFilterResult object containing word lists as well as validation result
         */
        public function validate(text:String, unescapeForeign:Boolean = true):Object{
            if (unescapeForeign) {
                text = escapeForeignChars(text);
            }
            text = removeNonStandardCharacters(text);
            var n:uint;
            var _matches:Array = [];
            var _indexes:Array = [];
            var _words:Array = [];
            if (!init) build();
            for (var a:uint = 0; a < formatedProfanityListHigh.length; a++) {
                var profanityA:String = formatedProfanityListHigh[a];
                var regExA:RegExp = new RegExp(profanityA, "gi");
                text.replace(regExA, function():String {
                    n = arguments.length-2;
                    _matches.push(arguments[0]);
                    _indexes.push([ arguments[n], arguments[n]+arguments[0].length ]);
                    _words.push(rawProfanityListHigh[a]);
                    return arguments[0];
                });
 
            }
            for (var b:uint = 0; b < formatedProfanityListLow.length; b++ ) {
                var profanityB:String = formatedProfanityListLow[b];
                var regExB:RegExp = new RegExp(profanityB, "gi");
                text.replace(regExB, function():String {
                    n = arguments.length-2;
                    _matches.push(arguments[0]);
                    _indexes.push([ arguments[n], arguments[n]+arguments[0].length ]);
                    _words.push(rawProfanityListLow[b]);
                    return arguments[0];
                });
            }
            var result:Object = new Object();
            result._clean = (_matches.length == 0);
            result._words = _words;
            result._matches = _matches;
            result._indexes = _indexes;
            return result;
        }
 
        /**
         * Quick validation that only returns true or false
         * @return will return false if any matches are found
         */
        public function quickValidate(text:String):Boolean{
            if (!init) build();
            for (var a:uint = 0; a < formatedProfanityListHigh.length; a++) {
                var profanityA:String = formatedProfanityListHigh[a];
                var regExA:RegExp = new RegExp(profanityA, "gi");
                if (regExA.test(text)) return false;
 
            }
            for (var b:uint = 0; b < formatedProfanityListLow.length; b++ ) {
                var profanityB:String = formatedProfanityListLow[b];
                var regExB:RegExp = new RegExp(profanityB, "gi");
                if (regExB.test(text)) return false;
            }
            return true;
        }
         
        /**
         * force the building of RegEx patterns 
         */
        public function build():void {
            if (!init) {
                if (!rawProfanityListHigh) setHighFromList(defaultListHigh);
                if (!rawProfanityListLow)  setLowFromList(defaultListLow);
                if (!rawIgnoreList)  setIgnoreFromList(defaultIgnoreList);
                buildLetterAssociations();
                formatedProfanityListHigh = convertProfanityList(rawProfanityListHigh);
                formatedProfanityListLow = convertProfanityList(rawProfanityListLow);
                formatedIgnoreList = convertProfanityList(rawIgnoreList);
                appendExactList();
                init = true;
            }
        }
         
        // setters /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
         
        /** 
         * Set the list of words to be matched even if in part of word
         * 
         * @param list list of words seperated by commas
         */
        public function setHighFromList(list:String):void {
            rawProfanityListHigh = list.split(",");
            init = false;
        }
         
        /** 
         * Set the list of words to be matched only when written alone, or as a plural, etc
         * 
         * @param list list of words seperated by commas
         */
        public function setLowFromList(list:String):void {
            rawProfanityListLow = list.split(",");
            init = false;
        }
 
        /** 
         * Set the list of words to be ignored if accidently matched
         * 
         * @param list list of words seperated by commas
         */
        public function setIgnoreFromList(list:String):void {
            rawIgnoreList = list.split(",");
            init = false;
        }
 
        /** 
         * Set the list of words to be matched even if in part of word
         * 
         * @param arr array of words to use
         */
        public function setHighFromArray(arr:Array):void {
            rawProfanityListHigh = arr;
            init = false;
        }
         
        /** 
         * Set the list of words to be matched even if in part of word
         * 
         * @param arr array of words to use
         */
        public function setLowFromArray(arr:Array):void {
            rawProfanityListLow = arr;
            init = false;
        }
         
        /** 
         * Set the list of words to be ignored if accidently matched
         * 
         * @param arr array of words to use
         */
        public function setIgnoreFromArray(arr:Array):void {
            rawIgnoreList = arr;
            init = false;
        }
         
        // getters /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
         
        /**
         * Get the list of words that will be always  be matched
         * 
         * @return list of words seperated by commas
         */
        public function getHighList():String {
            return rawProfanityListHigh.join(", ");
        }
         
        /**
         * Get the list of words that will only be matched alone or with suffixes
         * 
         * @return list of words seperated by commas
         */
        public function getLowList():String {
            return rawProfanityListLow.join(", ");
        }
         
        /**
         * Get the list of words that will be ignored
         * 
         * @return list of words seperated by commas
         */
        public function getIgnoreList():String {
            return rawIgnoreList.join(", ");
        }
 
        //internal utils //////////////////////////////////////////////////////////////////////////////////////////////////////////
         
        private function removeNonStandardCharacters(text:String):String {
            var pattern:RegExp;
            pattern = /[^\w\* @\|.,\&\[\]:;?\<\>~`\(\)\^%$#!\{\}\+\=-]/g;
            return text.replace(pattern,"*");
        }
         
        private function convertProfanityList(rawWordSet:Array):Array {
            var p:RegExp = new RegExp("( ){1,100}");
            var arr:Array = new Array();
            for (var i:uint = 0; i < rawWordSet.length; i++) {
                var word:String = rawWordSet[i];
                if (word && !p.test(word)) {
                    arr.push(getRegExString(word) );
                }
            }
            return arr;
        }
         
        private function buildLetterAssociations():void {
            letterAssociations = new Array();
            letterAssociations.push( ["a", "@"] );
            letterAssociations.push( ["e", "3"] );
            letterAssociations.push( ["i", "l", "1", "|"]);
            letterAssociations.push( ["o", "0"] );
            letterAssociations.push( ["s", "$", "5"] );
            letterAssociations.push( ["t", "7"] );
            letterAssociations.push( [" ", '( ?)'] );
            escaped = ["$","|",".","+","*","?","^","[","]","(",")","{","}","/","'","#","\\"];
        }
         
        /**
         * Turns a word into a string that can be used for a Regular Expression pattern
         * 
         * @param word String to be converted
         * @return string to be used in RegEx
         */
        public function getRegExString(word:String):String {
            var brokenWord:Array = word.split("");
            for (var i:uint = 0; i < brokenWord.length; i++) {
                var letter:String = brokenWord[i];
                for each (var association:Array in letterAssociations) {
                    for each (var char:String in association) {
                        if (letter.toLowerCase() == char.toLowerCase()) {
                            brokenWord[i] = buildRegEx(association);
                            break;
                        }else if (escaped.indexOf(brokenWord[i]) > -1) brokenWord[i] = "\\"+brokenWord[i];
                    }
                }
            }
            return brokenWord.join("");
        }
         
        private function buildRegEx(chars:Array):String {
            var build:String = "(";
            for (var i:uint = 0; i < chars.length; i++) {
                var char:String = chars[i];
                if (escaped.indexOf(char) > -1) build += "\\"+char;
                else build += char;
                if (i < chars.length-1) build += "|";
            }
            build +=")";
            return build;
        }   
         
        private function appendExactList() : void {
            for (var i:uint = 0;i < rawProfanityListLow.length; i++) {
                var word:String = formatedProfanityListLow[i];
                var raw:String = rawProfanityListLow[i];
                formatedProfanityListLow[i] = "("+expand(word,raw)+")(?<!(" + formatedIgnoreList.join("|") + "))";
            }
        }
         
        private function expand(regEx:String, raw:String):String {
            return "(?<![A-Za-z0-9])" + regEx + buildSuffexList(raw) + "(?![A-Za-z0-9])";
        }
         
        private function buildSuffexList(word:String):String {
            var preLast:String = word.charAt(word.length-2);
            var last:String = word.charAt(word.length-1);
            var y:String = "";
            if ( inList(last, "b,d,f,g,l,m,n,p,r,t,v,z") && isVowel(preLast) ) {
                y = "("+last+"?)";
            }
            return "((s|es|"+y+"er(s?)|ed|ing|"+y+"y)?)";
        }
         
        private function inList(item:String, list:String):Boolean {
            return list.split(",").indexOf(item) > -1;
        }
         
        private function isVowel(letter:String):Boolean {
            letter = letter.toLowerCase();
            return (letter == "a" || letter == "e" || letter == "i" || letter == "o" || letter == "u" || letter == "y");
        }
     
        // unescape foreign chars (ex: changes Ã¥ to a, Âµ to u)
         
        private function buildForeignChars():void {
            foreignChars = new Dictionary();
            var list:Array = [];
            list.push(["a","Ã¥,Ã¡,Ã¢,Ã¤,Ã£,Âª"]);
            list.push(["A","Ã…,Ã,Ã‚,Ã„,Ãƒ"]);
            list.push(["B","ÃŸ"]);
            list.push(["c","Ã§,Â¢"]);
            list.push(["d","âˆ‚"]);
            list.push(["e","Ã©,Ãª,Ã«"]);
            list.push(["E","Ã‰,ÃŠ,Ã‹,âˆ‘"]);
            list.push(["f","Æ’"]);
            list.push(["i","Ã­,Ã®,Ã¯,Â¡"]);
            list.push(["I","Ã,ÃŽ,Ã"]);
            list.push(["L","Â£"]);
            list.push(["n","Ã±"]);
            list.push(["N","Ã‘"]);
            list.push(["o","Ã¸,Ã³,Ã´,Ã¶,Ãµ,Âº"]);
            list.push(["O", "Ã˜,Ã“,Ã”,Ã–,Ã•"]);
            list.push(["R","Â®"]);
            list.push(["S","Â§"]);
            list.push(["t","â€ "]);
            list.push(["u","Ãº,Ã»,Ã¼,Âµ"]);
            list.push(["U","Ãš,Ã›,Ã›"]);
            list.push(["y","Ã¿"]);
            list.push(["Y","Å¸,Â¥"]);
            var a:Array = [];
            for each (var arr:Array in list) {
                var chars:Array = arr[1].split(",");
                for each (var char:String in chars) {
                    foreignChars[char] = arr[0];
                    a.push(char);
                }
            }
            var s:String = "("+a.join("|")+")";
            foreignCharRegEx = new RegExp(s,"g");
        }
 
        /**
         * Removes accented characters and the like, for example,changes Ã¥ to a, Âµ to u
         * 
         * @param text body of text to check and replace
         * @return new text without accents or foriegn chars
         */
        public function escapeForeignChars(text:String):String {
            if (!foreignChars) buildForeignChars();
            
            text = text.replace(foreignCharRegEx, function():String {
                return foreignChars[arguments[0]];
            });
            return text;
             
        }
         
    }
}