using UnityEngine;

public class GameTimeStamp : MonoBehaviour
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

        if(day > 30){
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
        //1 season = 30 days
        int seasonsInDays = (int)season;
        return seasonsInDays * 30;
    }

    //years to days
    public static int ConvertYearsToDays(int years){
        //1 year = 360 days
        return years * 4 * 30;
    }
}
