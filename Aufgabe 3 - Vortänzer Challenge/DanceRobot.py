"""
Directions:
0 = West,
1 = Northwest
2 = North
3 = Northeast
4 = East
5 = Southeast
6 = South
7 = Southwest
"""

class DanceRobot(object):
    viewDirection = 2

    def __init__(self, instructionSequence):
        self.x = 0
        self.y = 0
        self.program = DanceProgram(instructionSequence)

    def ResetPosition(self):
        self.x = 0
        self.y = 0
        self.viewDirection = 2

    def Update(self):
        nextInstruction = self.program.NextInstruction()

        if(nextInstruction == 0):
            self.x += self.GetXOffset()
            self.y += self.GetYOffset()
        elif(nextInstruction == 1):
            self.x -= self.GetXOffset()
            self.y -= self.GetYOffset()
        elif(nextInstruction == 2):
            if(self.viewDirection == 0):
                self.viewDirection = 7
            else:
                self.viewDirection -= 1
        elif(nextInstruction == 3):
            if(self.viewDirection == 7):
                self.viewDirection = 0
            else:
                self.viewDirection += 1
        elif(nextInstruction == 5):
            return True
        return False

    def GetXOffset(self):
        if(self.viewDirection == 4 or self.viewDirection == 3 or self.viewDirection == 5):
            return 1
        elif(self.viewDirection == 0 or self.viewDirection == 7 or self.viewDirection == 1):
            return -1
        return 0

    def GetYOffset(self):
        if(self.viewDirection == 6 or self.viewDirection == 7 or self.viewDirection == 5):
            return 1
        elif(self.viewDirection == 2 or self.viewDirection == 1 or self.viewDirection == 3):
            return -1
        return 0