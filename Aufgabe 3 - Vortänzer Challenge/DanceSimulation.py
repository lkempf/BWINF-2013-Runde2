class DanceSimulation(object):
    penaltyPoints = 0
    updateCount = 0

    def SetOriginalProgram(self, originalProgram):
        self.originalProgram = originalProgram;
        self.originalRobot = DanceRobot(originalProgram)
        self.originalRobotFinished = False
        self.finished = False
        self.updateCount = 0

    def SetImitatingProgram(self, imitatingProgram):
        self.imitatingProgram = imitatingProgram;
        self.imitatingRobot = DanceRobot(imitatingProgram)
        self.finished = False
        self.updateCount = 0

    def GetPenaltyPoints(self, zug):
        if(self.finished):
            return self.penaltyPoints
        else:
            while(not self.Update(zug)):
                pass
            self.finished = True
            return self.penaltyPoints

    def Update(self, zug):
        if self.updateCount == 254:
            return True
        
        zug.ausgabe(updateCount)
        self.updateCount += 1

        self.originalRobotFinished = originalRobotFinished = self.originalRobot.Update()
        imitatingRobotFinished = self.imitatingRobot.Update()
        robotsFinished = originalRobotFinished and imitatingRobotFinished

        if(not robotsFinished):
            self.penaltyPoints += max(abs(self.originalRobot.x - self.imitatingRobot.x), abs(self.originalRobot.y - self.imitatingRobot.y))

        zug.ausgabe(self.penaltyPoints)

        return robotsFinished
            
