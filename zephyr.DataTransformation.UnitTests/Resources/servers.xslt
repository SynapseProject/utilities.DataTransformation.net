<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <Servers>
      <xsl:for-each select="/Ec2Response/Ec2Instances/Ec2Instance">
        <Server>
          <xsl:value-of select="PrivateDnsName" />
        </Server>
      </xsl:for-each>
    </Servers>
  </xsl:template>
</xsl:stylesheet>