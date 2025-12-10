using UnityEngine;

[System.Serializable]
public class GameTimeStamp
{
    public int year;
    public enum Season{
        Spring,
        Summer,
        Fall,
        Winter
    }

    public Season season;

    public enum DayOfWeek{
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }
    
    public int day; 
    public int hour;
    public int minute;

    //set-up constructor
    public GameTimeStamp(int year, Season season, int day, int hour, int minute)
    {
        this.year = year;
        this.season = season;
        this.day = day;
        this.hour = hour;
        this.minute = minute; 
    }

    //create a copy of the currentTime
    public GameTimeStamp(GameTimeStamp currentTime)
    {
        this.year = currentTime.year;
        this.season = currentTime.season;
        this.day = currentTime.day;
        this.hour = currentTime.hour;
        this.minute = currentTime.minute; 
    }


    //increment the time by one minute
    public void UpdateClock(){
        minute++;
        if(minute>= 60) {
            //reset minutes
            minute = 0;
            hour++;
        }

        if(hour >= 24){
            //reset hour
            hour = 0;
            day++;
        }

        if(day > 7){
            //reset day
            day = 1;

            //increment season
            if (season == Season.Winter){
                season = Season.Spring;
                year++;
            } else {
                season++;
            }
        }
    }

    public DayOfWeek GetDayOfWeek(){
        //int dayPassed = ConvertDaysToHours(ConvertYearsToDays(year) + ConvertSeasonsToDays((int)season) + (day-1));

        int dayPassed = ConvertYearsToDays(year) + ConvertSeasonsToDays((int)season) + (day - 1);

        //Remainder when divided by 7 gives the day of the week
        int dayOfWeekIndex = dayPassed % 7;

        //Cast to DayOfWeek enum and return
        return (DayOfWeek)dayOfWeekIndex;
    }

//Conversion Methods
    //convert hours to minutes
    public static int ConvertHoursToMinutes(int hours){
        //1 hour = 60 minutes
        return hours * 60;
    }

    //convert days to hours
    public static int ConvertDaysToHours(int days){
        //1 day = 24 hours
        return days * 24;
    }

    //convert seasons to days
    public static int ConvertSeasonsToDays(int season){
        //1 season = 7 days
        int seasonsInDays = (int)season;
        return seasonsInDays * 7;
    }

    //years to days
    public static int ConvertYearsToDays(int years)
    {
        //1 year = 28 days
        return years * 4 * 7;
    }
    
    //The entire timestamp comparison in hours
    public static int TimestampInMinutes(GameTimeStamp t){
        return ConvertHoursToMinutes
        (ConvertDaysToHours(ConvertYearsToDays(t.year) + ConvertSeasonsToDays((int)t.season) + t.day) + t.hour) + t.minute;
    }

    public static int CompareTimeStamps(GameTimeStamp t1, GameTimeStamp t2){
        //convert timestamp to hours
        int t1InHours = ConvertDaysToHours(ConvertYearsToDays(t1.year)) + ConvertDaysToHours(ConvertSeasonsToDays((int)t1.season)) + ConvertDaysToHours(t1.day) + t1.hour;
        int t2InHours = ConvertDaysToHours(ConvertYearsToDays(t2.year)) + ConvertDaysToHours(ConvertSeasonsToDays((int)t2.season)) + ConvertDaysToHours(t2.day) + t2.hour;

        int difference = t2InHours - t1InHours;
        return Mathf.Abs(difference);
    }
}